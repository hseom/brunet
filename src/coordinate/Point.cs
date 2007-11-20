#define USE_HEIGHT
using System;

namespace Brunet.Coordinate {
  public class Point {
    protected static readonly int DIMENSIONS = 2;
    protected static readonly int INITIAL_VECTOR_VALUE  = 0;
    protected static readonly double MIN_HEIGHT = 0.01f;
    protected static readonly Random _rr = new Random();

    protected double[] _side;
    public double[] Side {
      get {
	return _side;
      } 
    }

    protected double _height;
    public double Height {
      get {
	return _height;
      }
      set {
	_height = value;
      }
    }
    
    public Point() {
      _side = new double[DIMENSIONS];
      for (int i = 0; i < _side.Length; i++) {
	_side[i] = INITIAL_VECTOR_VALUE;
      }
#if USE_HEIGHT
      _height = INITIAL_VECTOR_VALUE;
#endif
    }

    public Point(Point p):this(p.Side, p.Height) {}
    public Point(double[] side, double height) {
      if (side.Length != DIMENSIONS) {
	throw new Exception(String.Format("Only {0}-d points supported", DIMENSIONS));
      }
      _height = height;
      _side = new double[DIMENSIONS];
      for (int i = 0; i < DIMENSIONS; i++) {
	_side[i] = side[i];
      }
    }
    
    public Point(string s) {
      string[] ss = s.Split(new char[]{' '});
      _side = new double[DIMENSIONS];
      for (int i = 0; i < DIMENSIONS; i++) {
	_side[i] = double.Parse(ss[i].Trim());
      }
      _height = double.Parse(ss[DIMENSIONS].Trim());
    }

    public Point GetDirection(Point p) {
      double dist = GetEucledianDistance(p);
      if (dist == 0) {
	return null;
      }
      Point unitVector = new Point();
      for (int i = 0; i < DIMENSIONS; i++) {
	unitVector.Side[i] = (p.Side[i] - _side[i])/dist;
      }
#if USE_HEIGHT
      unitVector.Height = (_height + p.Height)/dist; 
#endif
      return unitVector;
    }
    public double GetEucledianDistance(Point p) {
      double d =  GetPlanarDistance(p);
#if USE_HEIGHT      
      d = d + _height + p.Height;
#endif
      return d;
    }
    public double GetPlanarDistance(Point p) {
      double sum = 0;
      for (int i = 0; i < DIMENSIONS; i++) {
	sum += (double) Math.Pow(_side[i] - p.Side[i], 2);
      } 
      return (double) Math.Sqrt(sum);
    }
    public double Length() {
      double sum = 0.0f;
      for (int i = 0; i < DIMENSIONS; i++) {
	sum += (double) Math.Pow(_side[i], 2);
      }
      double d = (double) Math.Sqrt(sum);
#if USE_HEIGHT
      d = d + _height;
#endif
      return d;
    }
    
    public void Bump() {
      for (int i = 0; i < DIMENSIONS; i++) {
	_side[i] = (double) _rr.NextDouble();
	Console.Error.WriteLine("after bump side ({0}): {1}", i, _side[i]);
      }
#if USE_HEIGHT      
      //only then do we bump the height (not otherwise)
      _height = (double) _rr.NextDouble();
      Console.Error.WriteLine("after bump height: {0}", _height);
#endif
    }
    
    public void Scale(double s) {
      for (int i = 0; i < DIMENSIONS; i++) {
	_side[i] = _side[i] * s;
      }
#if USE_HEIGHT
      _height = _height * s;
#endif
    }

    public void Add(Point p) {
      for (int i = 0; i < DIMENSIONS; i++) {
	_side[i] = _side[i] + p.Side[i];
      }
#if USE_HEIGHT
      _height = _height + p.Height;
#endif
    }

    public void CheckHeight() {
#if USE_HEIGHT
      while (_height < MIN_HEIGHT) {
	_height = (double) _rr.NextDouble();
      }
#endif
    }

    public static Point GetRandomUnitVector () {
      Point unitVector = new Point();
      double length = 0.0f;
      for (int i = 0; i < DIMENSIONS; i++) {
	unitVector.Side[i] = (double) _rr.NextDouble();
	length += unitVector.Side[i]*unitVector.Side[i];
      }  
      length = (double) Math.Sqrt(length);
      
      for (int i = 0; i < DIMENSIONS; i++) {
	unitVector.Side[i] /= length;
      }	
#if USE_HEIGHT
      unitVector.Height = 0.0f;
      unitVector.Height = (double) _rr.NextDouble();
#endif
      return unitVector;
    }
    public override string ToString() {
      string ss = "";
      for (int i= 0; i < DIMENSIONS; i++) {
	ss += _side[i];
	if (i < DIMENSIONS - 1) {
	  ss += " ";
	}
      }
#if USE_HEIGHT
      ss +=  (" " + _height);
#else
      ss += "";
#endif
      return ss;
    }

    public bool Equals(Point other) {
      if (other.Side.Length != Side.Length) {
	return false;
      }

      for (int i = 0; i < DIMENSIONS; i++) {
	if (other.Side[i] - Side[i] > 0.00001 ||
	    other.Side[i] - Side[i] < -0.00001) {
	  return false;
	}
      }
#if USE_HEIGHT      
      if (other.Height - Height > 0.00001 ||
	  other.Height - Height < -0.00001) {
	return false;
      }
#endif
      return true;
    }
  }
}
