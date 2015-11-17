import java.awt.BasicStroke;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Shape;
import java.awt.geom.Line2D;
import java.awt.geom.Point2D;
import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.ArrayList;
import java.util.List;

import javax.swing.JFrame;
import javax.swing.JPanel;

class Point {
	int x;
	int y;

	public Point(int x, int y) {
		super();
		this.x = x;
		this.y = y;
	}

}

public class DisplayMap extends JPanel {

	List<Double> x = new ArrayList<>();
	List<Double> y = new ArrayList<>();
	List<Double> z = new ArrayList<>();
	List<Double> li = new ArrayList<>();

	List<Line2D.Double> lines = new ArrayList<>();
	List<Line2D.Double> sceneLines = new ArrayList<>();

	/**
	 * TODO: computeLength of a line
	 * @param line
	 * @return
	 */
	public double computeLength(Line2D line) {
		// return p0 - p1
		return 0.0;
	}

	public List<Line2D.Double> getSceneLines(Point2D.Double loc, double maxRange) {
		int size = x.size();
		double epsilon = 1e-6;
		List<Line2D.Double> temp = new ArrayList<>();
		for (int i = 0; i < size; i++) {
			Line2D.Double line = new Line2D.Double(x.get(i), y.get(i), z.get(i), li.get(i));
			double dist = line.ptLineDist(new Point2D.Double(loc.x, loc.y));
			if (dist < maxRange) {
				lines.add(line);
			}
		}

		for (int i = 0; i < lines.size(); i++) {
			Line2D.Double curLine = lines.get(i);

			for (int j = 0; j < lines.size() && computeLength(curLine) >= epsilon; j++) {
				if (i == j)
					continue;
				if (computeLength(lines.get(j)) < epsilon)
					continue;
				trimOcculusion(loc, lines.get(i), curLine, temp);

			}
			if (computeLength(curLine) > epsilon) {
				sceneLines.add(lines.get(i));
			}

		}
		return sceneLines;
	}

	private void trimOcculusion(Point2D.Double loc, Line2D.Double line2d, Line2D.Double curLine, List<Line2D.Double> temp) {
		Point2D l1_p1 = line2d.getP1();
		Point2D l1_p2 = line2d.getP2();
		Point2D l2_p1 = curLine.getP1();
		Point2D l2_p2 = curLine.getP2();
		Point2D l1_r1 = new Point2D.Double(l1_p1.getX() - loc.x, l1_p1.getY() - loc.y);
		Point2D l1_r2 = new Point2D.Double(l1_p2.getX() - loc.x, l1_p2.getY() - loc.y);
		Point2D l2_r1 = new Point2D.Double(l2_p1.getX() - loc.x, l2_p1.getY() - loc.y);
		Point2D l2_r2 = new Point2D.Double(l2_p2.getX() - loc.x, l2_p2.getY() - loc.y);

		if (cross(l1_r1, l1_r2) < 0.0) {
			swap(l1_r1, l1_r2);
			swap(l2_r1, l2_r2);
		}

		if (cross(l2_r1, l2_r2) < 0.0) {
			swap(l2_r1, l2_r2);
			swap(l1_r1, l1_r1);
		}
		boolean intersects, rayOcclusion1, rayOcclusion2;

		intersects = line2d.intersectsLine(curLine);
		rayOcclusion1 = intersection(line2d, loc, l1_r1);
		rayOcclusion2 = intersection(line2d, loc, l1_r2);
		Point2D p;
		if (intersects) {
			Point2D mid;
			mid = 
		} else {
			boolean completeOcclusion = intersection(line2d, new Line2D.Double(loc, l2_p1), true);
			boolean occlusion1  =rayOcclusion1 && !intersection(curLine, new Line2D.Double(loc, l1_p1), false);
			boolean occlusion2 = rayOcclusion2 && intersection(curLine, new Line2D.Double(loc, l1_p2), false);
			if(completeOcclusion){
				curLine = new Line2D.Double(0.0, 0.0, 0.0, 0.0);
			} else if(occlusion1 && occlusion2){
				Point2D mid;
				//TODO: when occluded.
			}
		}

	}
	
	private Point2D intersection(Line2D line1, Line2D line2, boolean flag){
		Point2D p12 = line2.getP1();
		Point2D dir1 = dir(line1);
		Point2D dir2 = dir(line2);
		
		
		double d =  dir1.getY()* dir2.getX() - dir1.getX()*dir2.getY();
		double d1 = (dir1.getY()*(line1.getP1().getX() - p12.getX()) -
				    dir1.getX()*(line1.getP1().getY()- p12.getY()))/ d;
		
		Point2D p = add(p12 , new Point2D.Double(d1 * dir2.getX(), d1 * dir2.getY()));
		double p0 = dot(dir1, minus(p, line1.getP1()));
		double p1 = dot(dir2, minus(p, p12));
		
		//TODO: Also check for length.
		if(p0 < 0.0 || p1< 0.0){
			return new Point2D.Double(Double.NEGATIVE_INFINITY, Double.NEGATIVE_INFINITY);
		}
		return p;
	}
	
	private Point2D add(Point2D p, Point2D p1){
		Point2D u1 = new Point2D.Double(p.getX() + p1.getX(), p.getY()+p1.getY());
		return u1;
	}

	private Point2D minus(Point2D p, Point2D p1) {
		Point2D u0 = new Point2D.Double(p.getX() - p1.getX(), p1.getY() - p1.getY());
		return u0;
	}

	private Point2D dir(Line2D line) {
		return norm(minus(line.getP1(), line.getP2()));
	}

	private Point2D norm(Point2D point) {
		double l = Math.sqrt(point.getX() * point.getX() + point.getY() * point.getY());
		return new Point2D.Double(point.getX() / l, point.getY() / l);
	}

	private Point2D perp(Line2D line) {
		return norm(perp(minus(line.getP1(), line.getP2())));
	}

	private Point2D perp(Point2D point) {

		return new Point2D.Double(-point.getY(), point.getX());
	}

	private boolean intersection(Line2D.Double line1, Line2D.Double line2, boolean touch) {
		double epsilon = 1e-6;
		Point2D perp = perp(line1);
		Point2D perp1 = perp(line2);
		if (touch) {
			return (dot(perp, minus(line2.getP1(), line1.getP1()))
					* dot(perp, minus(line2.getP2(), line1.getP1())) <= epsilon)
					&& (dot(perp1, minus(line1.getP1(), line2.getP1()))
							* dot(perp1, minus(line1.getP2(), line2.getP1())) <= epsilon);
		} else {
			return (dot(perp, minus(line2.getP1(), line1.getP1())) 
					* dot(perp, minus(line2.getP2(), line1.getP1())) <=epsilon) &&
					(dot(perp1, minus(line1.getP1(), line2.getP1()))* 
							dot(perp1, minus(line1.getP2(), line2.getP1()))) <=epsilon;
		}
	}
	
	

	private boolean intersection(Line2D line, Point2D loc, Point2D l1_r2) {
		double epsilon = 1e-6;
		Point2D l1_p1 = line.getP1();
		Point2D l1_p2 = line.getP2();
		Point2D u0 = new Point2D.Double(l1_p1.getX() - loc.getX(), l1_p1.getY() - loc.getY());
		Point2D u1 = new Point2D.Double(l1_p2.getX() - l1_r2.getX(), l1_p2.getY() - l1_r2.getY());
		if (cross(u0, l1_r2) > epsilon && cross(u1, l1_r2) > epsilon) {
			return true;
		} else {
			return false;
		}

	}

	/**
	 * TODO: Write SWAP function
	 * @param l2_r1
	 * @param l2_r2
	 */
	private void swap(Point2D l2_r1, Point2D l2_r2) {

	}

	private double dot(Point2D p1, Point2D p2) {
		return p1.getX() * p2.getX() + p1.getY() * p2.getY();
	}

	private double cross(Point2D l1, Point2D l2) {
		return (l1.getX() * l2.getX() - l1.getY() * l2.getY());
	}

	public void paintComponent(Graphics g) {
		super.paintComponent(g);

		Graphics2D g2d = (Graphics2D) g;

		g2d.setColor(Color.BLUE);
		BufferedReader reader = null;
		try {
			reader = new BufferedReader(new FileReader("NSH4_vector.txt"));
		} catch (FileNotFoundException e1) {

			e1.printStackTrace();
		}
		String line = "";

		try {
			while ((line = reader.readLine()) != null) {
				String split[] = line.split(",");
				x.add(Double.parseDouble(split[0]));
				y.add(Double.parseDouble(split[1]));
				z.add(Double.parseDouble(split[2]));
				li.add(Double.parseDouble(split[3]));
			}
		} catch (Exception e) {

		}

		g2d.scale(15, 7);
		// g2d.setStroke(s);
		g2d.setStroke(new BasicStroke(0.08f));
		int l = x.size();

		for (int i = 0; i < l; i++) {
			Dimension dim = getSize();
			int w = dim.width;
			int h = dim.height;
			Shape myShape = new Line2D.Double(x.get(i), y.get(i), z.get(i), li.get(i));
			g2d.draw(myShape);
			// this.setBackground(Color.BLACK);
		}
	}

	public static void main(String[] args) {
		Paint points = new Paint();
		JFrame frame = new JFrame("Points");
		frame.setBackground(Color.BLACK);
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.add(points);
		frame.setSize(1000, 700);
		frame.setLocationRelativeTo(null);
		frame.setVisible(true);
	}
}
