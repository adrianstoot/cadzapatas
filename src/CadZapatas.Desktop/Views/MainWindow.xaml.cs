using System.Windows;
using System.Windows.Media.Media3D;
using CadZapatas.Desktop.ViewModels;
using CadZapatas.Foundations;
using CadZapatas.Retaining;
using HelixToolkit.Wpf;

namespace CadZapatas.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => Rebuild3D();
        DataContextChanged += (_, _) => Rebuild3D();
        if (DataContext is MainViewModel vm)
            vm.PropertyChanged += (_, _) => Rebuild3D();
    }

    /// <summary>Reconstruye el contenido 3D del viewport a partir del ViewModel.</summary>
    public void Rebuild3D()
    {
        if (DataContext is not MainViewModel vm) return;
        if (Viewport == null) return;

        // Mantiene luces y ejes; elimina solo geometrias calculadas
        for (int i = Viewport.Children.Count - 1; i >= 0; i--)
        {
            var c = Viewport.Children[i];
            if (c is ModelVisual3D mv && mv.GetValue(TagProperty) is "BimGeom")
                Viewport.Children.RemoveAt(i);
        }

        foreach (var f in vm.Footings) AddFooting(f);
        foreach (var w in vm.Walls)    AddWall(w);
        foreach (var p in vm.Piles)    AddPile(p);
    }

    private static readonly DependencyProperty TagProperty =
        DependencyProperty.RegisterAttached("BimTag", typeof(string), typeof(MainWindow));

    private void AddFooting(IsolatedFooting f)
    {
        var mb = new MeshBuilder();
        mb.AddBox(new Point3D(f.InsertionPoint.X, f.InsertionPoint.Y,
                              f.InsertionPoint.Z - f.Thickness / 2),
                  f.Length, f.Width, f.Thickness);
        var geom = new GeometryModel3D
        {
            Geometry = mb.ToMesh(),
            Material = Materials.Gray,
            BackMaterial = Materials.Gray
        };
        var mv = new ModelVisual3D { Content = geom };
        mv.SetValue(TagProperty, "BimGeom");
        Viewport.Children.Add(mv);
    }

    private void AddWall(RetainingWall w)
    {
        var mb = new MeshBuilder();
        double h = w.Height;
        double t = (w.StemThicknessTop + w.StemThicknessBottom) / 2;
        mb.AddBox(new Point3D(w.StartPoint.X, w.StartPoint.Y, w.StartPoint.Z + h / 2),
                  Math.Max(w.WallLength, 4.0), t, h);
        mb.AddBox(new Point3D(w.StartPoint.X, w.StartPoint.Y,
                              w.StartPoint.Z - w.FoundationThickness / 2),
                  Math.Max(w.WallLength, 4.0), w.BaseWidth, w.FoundationThickness);
        var geom = new GeometryModel3D
        {
            Geometry = mb.ToMesh(),
            Material = Materials.LightGray,
            BackMaterial = Materials.LightGray
        };
        var mv = new ModelVisual3D { Content = geom };
        mv.SetValue(TagProperty, "BimGeom");
        Viewport.Children.Add(mv);
    }

    private void AddPile(Pile p)
    {
        var mb = new MeshBuilder();
        mb.AddCylinder(new Point3D(p.InsertionPoint.X, p.InsertionPoint.Y, p.TipElevation),
                        new Point3D(p.InsertionPoint.X, p.InsertionPoint.Y, p.HeadElevation),
                        p.Diameter / 2, 24);
        var geom = new GeometryModel3D
        {
            Geometry = mb.ToMesh(),
            Material = Materials.Brown,
            BackMaterial = Materials.Brown
        };
        var mv = new ModelVisual3D { Content = geom };
        mv.SetValue(TagProperty, "BimGeom");
        Viewport.Children.Add(mv);
    }
}
