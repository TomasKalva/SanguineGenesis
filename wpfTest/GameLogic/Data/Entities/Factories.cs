﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic
{
    public abstract class Factories<Factory> where Factory : EntityFactory
    {
        public Dictionary<string, Factory> Factorys { get; }
        protected Dictionary<string, string> abilitiesList;

        public Factories()
        {
            Factorys = new Dictionary<string, Factory>();
            abilitiesList = new Dictionary<string, string>();
        }

        public Factory this[string entityType] 
        {
            get
            {
                if (Factorys.TryGetValue(entityType, out Factory factory))
                    return factory;
                else
                    throw new ArgumentException("There is no "+ typeof(Factory)+" for " + entityType);
            }
        }

        public void InitFactorys(string fileName, Statuses statuses)
        {
            using (StreamReader fileReader = new StreamReader(fileName))
            {
                //first line is just a description of the format
                string line = fileReader.ReadLine();
                while ((line = fileReader.ReadLine()) != null)
                {
                    AddNewFactory(line, statuses);
                }
            }
        }

        public abstract void AddNewFactory(string description, Statuses statuses);

        public List<StatusFactory> ParseStatuses(string listOfStatuses, Statuses statuses)
        {
            List<StatusFactory> statusFactories = new List<StatusFactory>();
            string[] statusesNames = listOfStatuses.Split(';');
            foreach(string stName in statusesNames)
            {
                switch (stName)
                {
                    case "underground":
                        statusFactories.Add(statuses.UndergroundFactory);
                        break;
                }
            }
            return statusFactories;
        }

        public void InitAbilities(Abilities abilities)
        {
            foreach(var entityAbilities in abilitiesList)
            {
                Factory factory = this[entityAbilities.Key];
                string[] abilitiesDesc = entityAbilities.Value.Split(';');
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
                            case "rallyPoint":
                                factory.Abilities.Add(abilities.SetRallyPoint);
                                break;
                            case "eat":
                                //eat command can only be added to an animal
                                var animF = factory as AnimalFactory;
                                if(animF!=null)
                                {
                                    if(animF.Diet==Diet.HERBIVORE)
                                        factory.Abilities.Add(abilities.HerbivoreEat);
                                    else
                                        factory.Abilities.Add(abilities.CarnivoreEat);
                                }
                                break;
                            case "poisonousSpit":
                                factory.Abilities.Add(abilities.PoisonousSpit);
                                break;
                            case "activateSprint":
                                factory.Abilities.Add(abilities.ActivateSprint);
                                break;
                            case "piercingBite":
                                factory.Abilities.Add(abilities.PiercingBite);
                                break;
                            case "consumeAnimal":
                                factory.Abilities.Add(abilities.ConsumeAnimal);
                                break;
                            case "jump":
                                factory.Abilities.Add(abilities.Jump);
                                break;
                            case "activateShell":
                                factory.Abilities.Add(abilities.ActivateShell);
                                break;
                            case "pull":
                                factory.Abilities.Add(abilities.Pull);
                                break;
                            case "bigPull":
                                factory.Abilities.Add(abilities.BigPull);
                                break;
                            case "farSight":
                                factory.Abilities.Add(abilities.ActivateFarSight);
                                break;
                            case "knockBack":
                                factory.Abilities.Add(abilities.KnockBack);
                                break;
                            case "climbTree":
                                factory.Abilities.Add(abilities.ClimbTree);
                                break;
                            case "enterHole":
                                factory.Abilities.Add(abilities.EnterHole);
                                break;
                            case "exitHole":
                                factory.Abilities.Add(abilities.ExitHole);
                                break;
                            case "fastStrikes":
                                factory.Abilities.Add(abilities.ActivateFastStrikes);
                                break;
                            case "improveStructure":
                                factory.Abilities.Add(abilities.ImproveStructure);
                                break;
                            case "chargeTo":
                                factory.Abilities.Add(abilities.ChargeTo);
                                break;
                            case "kick":
                                factory.Abilities.Add(abilities.Kick);
                                break;
                        }
                    }else if (abPar.Length == 2)
                    {
                        switch (abName)
                        {
                            case "build":
                                factory.Abilities.Add(abilities.BuildBuilding(abPar[1]));
                                break;
                            case "spawn":
                                factory.Abilities.Add(abilities.UnitSpawn(abPar[1]));
                                break;
                            case "create":
                                factory.Abilities.Add(abilities.UnitCreate(abPar[1]));
                                break;
                        }
                    }
                }
            }
        }
    }

    public class TreeFactories : Factories<TreeFactory>
    {
        public override void AddNewFactory(string description, Statuses statuses)
        {
            string[] fields = description.Split(',');
            string treeType = fields[0];
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
            List<StatusFactory> statusFactories = ParseStatuses(fields[12], statuses);
            bool producer = fields[13] == "yes";
            int air = int.Parse(fields[14]);

            TreeFactory newFactory = new TreeFactory(treeType, maxHealth, maxEnergy, energyRegen, size, physical, energyCost,
                biome, terrain, soilQuality, producer, 10f, rootsDistance, air, statusFactories);
            Factorys.Add(treeType, newFactory);
            abilitiesList.Add(treeType, fields[11]);
        }
    }

    public class StructureFactories : Factories<StructureFactory>
    {
        public override void AddNewFactory(string description, Statuses statuses)
        {
            string[] fields = description.Split(',');
            string structureType = fields[0];
            decimal maxHealth = decimal.Parse(fields[1]);
            decimal maxEnergy = decimal.Parse(fields[2]);
            bool physical = fields[3] == "yes";
            int size = int.Parse(fields[4]);
            decimal energyCost = decimal.Parse(fields[5]);
            Biome biome = (Biome)Enum.Parse(typeof(Biome), fields[6]);
            Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain), fields[7]);
            SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality), fields[8]);
            List<StatusFactory> statusFactories = ParseStatuses(fields[10], statuses);
            bool producer = fields[11] == "yes";

            StructureFactory newFactory = new StructureFactory(structureType, maxHealth, maxEnergy, size, physical, energyCost,
                biome, terrain, soilQuality,  producer, 10f, statusFactories);
            Factorys.Add(structureType, newFactory);
            abilitiesList.Add(structureType, fields[9]);
        }
    }

    public class UnitFactories : Factories<AnimalFactory>
    {
        public override void AddNewFactory(string description, Statuses statuses)
        {
            string[] fields = description.Split(',');
            string unitType = fields[0];
            decimal maxHealth = decimal.Parse(fields[1]);
            decimal maxEnergy = decimal.Parse(fields[2]);
            decimal foodEnergyRegen = decimal.Parse(fields[3]);
            float foodEatingPeriod = float.Parse(fields[4]);
            float range = float.Parse(fields[5]);
            decimal energyCost = decimal.Parse(fields[6]);
            decimal attackDamage = decimal.Parse(fields[7]);
            float attackDistance = float.Parse(fields[8]);
            float attackPeriod = float.Parse(fields[9]);
            bool mechanicalDamage = fields[10] == "yes";
            //field[11] isn't important
            float maxSpeedLand = float.Parse(fields[12]);
            float maxSpeedWater = float.Parse(fields[13]);
            Movement movement = (Movement)Enum.Parse(typeof(Movement), fields[14]);
            bool thickSkin = fields[15] == "yes";
            Diet diet = (Diet)Enum.Parse(typeof(Diet), fields[16]);
            float spawningTime = float.Parse(fields[17]);
            List<StatusFactory> statusFactories = ParseStatuses(fields[19], statuses);
            int air = int.Parse(fields[20]);

            Factorys.Add(unitType, 
                new AnimalFactory(
                    unitType: unitType,
                    maxHealth: maxHealth,
                    maxEnergy: maxEnergy,
                    foodEnergyRegen: foodEnergyRegen,
                    foodEatingPeriod: foodEatingPeriod,
                    range: range,
                    attackDamage: attackDamage,
                    attackPeriod: attackPeriod,
                    attackDistance: attackDistance,
                    mechanicalDamage: mechanicalDamage,
                    maxSpeedLand: maxSpeedLand,
                    maxSpeedWater: maxSpeedWater,
                    movement: movement,
                    thickSkin: thickSkin,
                    diet: diet,
                    spawningTime: spawningTime,
                    physical: true,
                    energyCost: energyCost,
                    viewRange: 5,
                    statusFactories: statusFactories,
                    air:air));
            //new UnitFactory(string.TIGER, 0.5f,2f,2f,100,10,Movement.LAND,4f););

            abilitiesList.Add(unitType, fields[19]);
        }
    }
}
