﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public abstract class Factories<Factory> where Factory : EntityFactory
    {
        protected Dictionary<EntityType, Factory> factories;
        protected Dictionary<EntityType, string> abilitiesList;
        public Factory this[EntityType entityType] 
        {
            get
            {
                if (factories.TryGetValue(entityType, out Factory factory))
                    return factory;
                else
                    throw new ArgumentException("There is no "+ typeof(Factory)+" for " + entityType);
            }
        }

        public void InitFactories(string fileName)
        {
            factories = new Dictionary<EntityType, Factory>();
            abilitiesList = new Dictionary<EntityType, string>();
            using (StreamReader fileReader = new StreamReader(fileName))
            {
                string line = fileReader.ReadLine();//first line is just a description
                while ((line = fileReader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    AddNewFactory(line);
                }
            }
        }

        public abstract void AddNewFactory(string description);

        public void InitAbilities(Abilities abilities)
        {
            foreach(var entityAbilities in abilitiesList)
            {
                Factory factory = this[entityAbilities.Key];
                string[] abilitiesDesc = entityAbilities.Value.Split(';');
                Console.WriteLine(entityAbilities.Key);
                foreach(string abilityDesc in abilitiesDesc)
                {
                    string[] abPar = abilityDesc.Split(':');
                    string abName = abPar[0];
                    if (abPar.Length == 1)
                    {
                        switch (abName)
                        {
                            case "moveTo":
                                 factory.Abilities.Add(abilities.MoveTo);
                                break;
                            case "attack":
                                factory.Abilities.Add(abilities.Attack);
                                break;
                        }
                    }else if (abPar.Length == 2)
                    {
                        switch (abName)
                        {
                            case "build":
                                if (entityAbilities.Key == EntityType.BANNANA_TREE)
                                    ;
                                factory.Abilities.Add(abilities.PlantBuilding((EntityType)Enum.Parse(typeof(EntityType), abPar[1])));
                                break;
                            /*case "spawn":
                                factory.Abilities.Add(abilities.UnitSpawn((EntityType)Enum.Parse(typeof(EntityType), abPar[1])));
                                break;*/
                        }
                    }
                }
            }
        }
    }

    public class TreeFactories : Factories<TreeFactory>
    {
        public override void AddNewFactory(string description)
        {
            string[] fields = description.Split(',');
            EntityType treeType = (EntityType)Enum.Parse(typeof(EntityType), fields[0]);
            decimal maxHealth=decimal.Parse(fields[1]);
            decimal maxEnergy = decimal.Parse(fields[2]);
            decimal energyRegen = decimal.Parse(fields[3]);
            bool physical = fields[4] == "yes";
            int size = int.Parse(fields[5]);
            int rootsDistance = int.Parse(fields[6]);
            decimal energyCost = decimal.Parse(fields[7]);
            Biome biome = (Biome)Enum.Parse(typeof(Biome),fields[8]);
            Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain),fields[9]);
            SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality),fields[10]);

            factories.Add(treeType, new TreeFactory(treeType, maxHealth, maxEnergy, energyRegen, size, physical, energyCost,
                biome, terrain, soilQuality, false,  10f, rootsDistance, 2  ));
            abilitiesList.Add(treeType, fields[11]);
        }
    }

    public class UnitFactories : Factories<UnitFactory>
    {
        public override void AddNewFactory(string description)
        {
            string[] fields = description.Split(',');
            EntityType treeType = (EntityType)Enum.Parse(typeof(EntityType), fields[0]);
            /*decimal maxHealth = decimal.Parse(fields[1]);
            decimal maxEnergy = decimal.Parse(fields[2]);
            decimal energyRegen = decimal.Parse(fields[3]);
            bool physical = fields[4] == "yes";
            int size = int.Parse(fields[5]);
            int rootsDistance = int.Parse(fields[6]);
            decimal energyCost = decimal.Parse(fields[7]);
            Biome biome = (Biome)Enum.Parse(typeof(Biome), fields[8]);
            Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain), fields[9]);
            SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality), fields[10]);*/

            factories.Add(treeType, new UnitFactory(EntityType.TIGER, 200, 150, 0.5f, true, 30m, 5f, 2f, 4f, Movement.LAND_WATER, 15f, 5m, 0.3f, 0.1f));
            //abilitiesList.Add(treeType, fields[11]);
        }
    }
}
