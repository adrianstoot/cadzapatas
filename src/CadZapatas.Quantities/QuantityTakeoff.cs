using CadZapatas.Foundations;
using CadZapatas.Reinforcement;
using CadZapatas.Retaining;

namespace CadZapatas.Quantities;

/// <summary>
/// Motor de mediciones automaticas. Recorre el modelo BIM y genera partidas por:
///  - hormigon de limpieza
///  - hormigon estructural (diferenciado por clase)
///  - acero corrugado (diferenciado por diametro)
///  - encofrado (superficie)
///  - excavacion (volumen).
/// Precios unitarios editables o tomados de la base de precios configurada.
/// </summary>
public class QuantityTakeoff
{
    public Dictionary<string, double> UnitPrices { get; set; } = new()
    {
        ["HL-150"] = 65.00,     // EUR/m3
        ["HM-20"] = 75.00,
        ["HA-25"] = 95.00,
        ["HA-30"] = 102.00,
        ["HA-35"] = 110.00,
        ["B500SD"] = 1.10,      // EUR/kg
        ["B500T_mallazo"] = 1.20,
        ["encofrado"] = 25.00,  // EUR/m2
        ["excavacion"] = 8.00   // EUR/m3
    };

    public Budget GenerateBudget(string projectName,
                                  IEnumerable<Foundation> foundations,
                                  IEnumerable<RetainingWall> walls,
                                  IEnumerable<ReinforcementLayout> reinforcement)
    {
        var b = new Budget { ProjectName = projectName };
        int seq = 1;

        foreach (var f in foundations)
        {
            AddFoundationItems(b, f, ref seq);
        }
        foreach (var w in walls)
        {
            AddWallItems(b, w, ref seq);
        }
        foreach (var r in reinforcement)
        {
            AddReinforcementItems(b, r, ref seq);
        }

        return b;
    }

    private void AddFoundationItems(Budget b, Foundation f, ref int seq)
    {
        double vConc = 0, vLean = 0;
        double aForm = 0;

        if (f is IsolatedFooting ifoot)
        {
            vConc = ifoot.VolumeConcrete;
            vLean = ifoot.LeanConcreteVolume;
            aForm = ifoot.FormworkArea;
        }
        else if (f is StripFooting sf)
        {
            vConc = sf.VolumeConcrete;
            vLean = sf.LeanConcreteVolume;
            aForm = sf.FormworkArea;
        }
        else if (f is MatFoundation mat)
        {
            vConc = mat.VolumeConcrete;
            vLean = mat.PlanArea * mat.LeanConcreteThickness;
        }
        else if (f is Pile pile)
        {
            vConc = pile.VolumeConcrete;
        }
        else if (f is PileCap cap)
        {
            vConc = cap.VolumeConcrete;
            vLean = cap.LeanConcreteVolume;
        }

        if (vLean > 0)
            b.Items.Add(new QuantityItem
            {
                Code = $"03HL{seq++:D5}",
                Description = $"m3 Hormigon de limpieza HL-150 bajo {f.ObjectType} {f.Code}",
                Unit = "m3",
                Quantity = Math.Round(vLean, 3),
                UnitPrice = UnitPrices.GetValueOrDefault("HL-150", 65),
                Category = "Cimentacion",
                ElementId = f.Id.ToString()
            });

        if (vConc > 0)
        {
            string concrete = f.ConcreteClass.Split('-')[0] + "-" + f.ConcreteClass.Split('-')[1].Split('/')[0];
            b.Items.Add(new QuantityItem
            {
                Code = $"03HA{seq++:D5}",
                Description = $"m3 Hormigon estructural {f.ConcreteClass} en {f.ObjectType} {f.Code}",
                Unit = "m3",
                Quantity = Math.Round(vConc, 3),
                UnitPrice = UnitPrices.GetValueOrDefault(concrete,
                            UnitPrices.GetValueOrDefault("HA-25", 95)),
                Category = "Cimentacion",
                ElementId = f.Id.ToString()
            });
        }

        if (aForm > 0)
            b.Items.Add(new QuantityItem
            {
                Code = $"03EN{seq++:D5}",
                Description = $"m2 Encofrado recto plano en {f.ObjectType} {f.Code}",
                Unit = "m2",
                Quantity = Math.Round(aForm, 2),
                UnitPrice = UnitPrices.GetValueOrDefault("encofrado", 25),
                Category = "Cimentacion",
                ElementId = f.Id.ToString()
            });
    }

    private void AddWallItems(Budget b, RetainingWall w, ref int seq)
    {
        b.Items.Add(new QuantityItem
        {
            Code = $"03HA{seq++:D5}",
            Description = $"m3 Hormigon estructural {w.ConcreteClass} en muro {w.Code}",
            Unit = "m3",
            Quantity = Math.Round(w.TotalConcreteVolume, 3),
            UnitPrice = UnitPrices.GetValueOrDefault("HA-25", 95),
            Category = "Contencion",
            ElementId = w.Id.ToString()
        });
        b.Items.Add(new QuantityItem
        {
            Code = $"03EN{seq++:D5}",
            Description = $"m2 Encofrado dos caras en muro {w.Code}",
            Unit = "m2",
            Quantity = Math.Round(w.BackfaceExposedArea * 2, 2),
            UnitPrice = UnitPrices.GetValueOrDefault("encofrado", 25),
            Category = "Contencion",
            ElementId = w.Id.ToString()
        });
    }

    private void AddReinforcementItems(Budget b, ReinforcementLayout r, ref int seq)
    {
        foreach (var grp in r.Bars.GroupBy(x => new { x.DiameterMm, x.SteelGrade }))
        {
            double kg = grp.Sum(x => x.TotalWeightKg);
            b.Items.Add(new QuantityItem
            {
                Code = $"03AC{seq++:D5}",
                Description = $"kg Acero {grp.Key.SteelGrade} Ø{grp.Key.DiameterMm} mm",
                Unit = "kg",
                Quantity = Math.Round(kg, 1),
                UnitPrice = UnitPrices.GetValueOrDefault("B500SD", 1.10),
                Category = "Armaduras",
                ElementId = r.OwnerElementId.ToString()
            });
        }
        double kgStirrups = r.Stirrups.Sum(s => s.TotalWeightKg);
        if (kgStirrups > 0)
            b.Items.Add(new QuantityItem
            {
                Code = $"03ACE{seq++:D4}",
                Description = "kg Acero B500SD en cercos y estribos",
                Unit = "kg",
                Quantity = Math.Round(kgStirrups, 1),
                UnitPrice = UnitPrices.GetValueOrDefault("B500SD", 1.10),
                Category = "Armaduras",
                ElementId = r.OwnerElementId.ToString()
            });
        double kgMesh = r.Meshes.Sum(m => m.TotalWeightKg);
        if (kgMesh > 0)
            b.Items.Add(new QuantityItem
            {
                Code = $"03ACM{seq++:D4}",
                Description = "kg Mallazo electrosoldado B500T",
                Unit = "kg",
                Quantity = Math.Round(kgMesh, 1),
                UnitPrice = UnitPrices.GetValueOrDefault("B500T_mallazo", 1.20),
                Category = "Armaduras",
                ElementId = r.OwnerElementId.ToString()
            });
    }
}
