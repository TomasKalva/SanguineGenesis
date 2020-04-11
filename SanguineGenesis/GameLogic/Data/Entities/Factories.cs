using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Loads and stores factories of the type EntityFactory.
    /// </summary>
    abstract class Factories<Factory> where Factory : EntityFactory
    {
        /// <summary>
        /// For each entity name created by Factory contains a factory that
        /// creates the entity.
        /// </summary>
        public Dictionary<string, Factory> Factorys { get; }
        /// <summary>
        /// For each entity name contains list of ability names separated by ',' that
        /// this entity factory will parse.
        /// </summary>
        protected Dictionary<string, string> abilitiesList;

        public Factories()
        {
            Factorys = new Dictionary<string, Factory>();
            abilitiesList = new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns factory for the entity type.
        /// </summary>
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

        /// <summary>
        /// Load Factorys and abilities from the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="statuses">Statuses that can be given to factories that require them.</param>
        public void InitFactorys(string fileName, Statuses.Statuses statuses)
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

        /// <summary>
        /// Adds a new factory to the Factorys.
        /// </summary>
        /// <param name="description">String that describes the factory's properties.</param>
        /// <param name="statuses">Statuses that can be given to the factory if it requires them.</param>
        public abstract void AddNewFactory(string description, Statuses.Statuses statuses);

        /// <summary>
        /// Returns a list of statuses represented by the string listOfStatuses.
        /// </summary>
        public List<StatusFactory> ParseStatuses(string listOfStatuses, Statuses.Statuses statuses)
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

        /// <summary>
        /// Uses abilitiesList to set abilities to the already created Factorys. Has to be called after
        /// InitFactorys.
        /// </summary>
        public void InitAbilities(Abilities.Abilities abilities)
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
                        //abilities with no parameters
                        switch (abName)
                        {
                            case "unbreakableMoveTo":
                                 factory.Abilities.Add(abilities.UnbreakableMoveTo);
                                break;
                            case "moveTo":
                                factory.Abilities.Add(abilities.MoveTo);
                                break;
                            case "unbreakableAttack":
                                factory.Abilities.Add(abilities.UnbreakableAttack);
                                break;
                            case "attack":
                                factory.Abilities.Add(abilities.Attack);
                                break;
                            case "rallyPoint":
                                factory.Abilities.Add(abilities.SetRallyPoint);
                                break;
                            case "eat":
                                //eat command can only be added to an animal
                                if (factory is AnimalFactory animF)
                                {
                                    if (animF.Diet == Diet.HERBIVORE)
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
                        //abilities with one parameter
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

    /// <summary>
    /// Loads and stores factories of the type TreeFactory.
    /// </summary>
    class TreeFactories : Factories<TreeFactory>
    {
        public override void AddNewFactory(string description, Statuses.Statuses statuses)
        {
            string[] fields = description.Split(',');
            string treeType = fields[0];
            float maxHealth=float.Parse(fields[1], CultureInfo.InvariantCulture);
            float maxEnergy = float.Parse(fields[2], CultureInfo.InvariantCulture);
            float energyRegen = float.Parse(fields[3], CultureInfo.InvariantCulture);
            bool physical = fields[4] == "yes";
            int size = int.Parse(fields[5], CultureInfo.InvariantCulture);
            int rootsDistance = int.Parse(fields[6], CultureInfo.InvariantCulture);
            float energyCost = float.Parse(fields[7], CultureInfo.InvariantCulture);
            Biome biome = (Biome)Enum.Parse(typeof(Biome),fields[8]);
            Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain),fields[9]);
            SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality),fields[10]);
            List<StatusFactory> statusFactories = ParseStatuses(fields[12], statuses);
            bool producer = fields[13] == "yes";
            int air = int.Parse(fields[14], CultureInfo.InvariantCulture);
            float buildingDistance = float.Parse(fields[15], CultureInfo.InvariantCulture);
            float viewRange = float.Parse(fields[16], CultureInfo.InvariantCulture);
            bool blocksVision = fields[17] == "yes";

            TreeFactory newFactory = new TreeFactory(treeType, maxHealth, maxEnergy, energyRegen, size, physical, energyCost,
                biome, terrain, soilQuality, producer, buildingDistance, viewRange, blocksVision, rootsDistance, air, statusFactories);
            Factorys.Add(treeType, newFactory);
            abilitiesList.Add(treeType, fields[11]);
        }
    }

    /// <summary>
    /// Loads and stores factories of the type StructureFactory.
    /// </summary>
    class StructureFactories : Factories<StructureFactory>
    {
        public override void AddNewFactory(string description, Statuses.Statuses statuses)
        {
            string[] fields = description.Split(',');
            string structureType = fields[0];
            float maxHealth = float.Parse(fields[1], CultureInfo.InvariantCulture);
            float maxEnergy = float.Parse(fields[2], CultureInfo.InvariantCulture);
            bool physical = fields[3] == "yes";
            int size = int.Parse(fields[4], CultureInfo.InvariantCulture);
            float energyCost = float.Parse(fields[5], CultureInfo.InvariantCulture);
            Biome biome = (Biome)Enum.Parse(typeof(Biome), fields[6]);
            Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain), fields[7]);
            SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality), fields[8]);
            List<StatusFactory> statusFactories = ParseStatuses(fields[10], statuses);
            bool producer = fields[11] == "yes";
            float buildingDistance = float.Parse(fields[12], CultureInfo.InvariantCulture);
            float viewRange = float.Parse(fields[13], CultureInfo.InvariantCulture);
            bool blocksVision = fields[14] == "yes";

            StructureFactory newFactory = new StructureFactory(structureType, maxHealth, maxEnergy, size, physical, energyCost,
                biome, terrain, soilQuality,  producer, buildingDistance, viewRange, blocksVision, statusFactories);
            Factorys.Add(structureType, newFactory);
            abilitiesList.Add(structureType, fields[9]);
        }
    }

    /// <summary>
    /// Loads and stores factories of the type AnimalFactory.
    /// </summary>
    class AnimalFactories : Factories<AnimalFactory>
    {
        public override void AddNewFactory(string description, Statuses.Statuses statuses)
        {
            string[] fields = description.Split(',');
            string unitType = fields[0];
            float maxHealth = float.Parse(fields[1], CultureInfo.InvariantCulture);
            float maxEnergy = float.Parse(fields[2], CultureInfo.InvariantCulture);
            float foodEnergyRegen = float.Parse(fields[3], CultureInfo.InvariantCulture);
            float foodEatingPeriod = float.Parse(fields[4], CultureInfo.InvariantCulture);
            float radius = float.Parse(fields[5], CultureInfo.InvariantCulture);
            float energyCost = float.Parse(fields[6], CultureInfo.InvariantCulture);
            float attackDamage = float.Parse(fields[7], CultureInfo.InvariantCulture);
            float attackDistance = float.Parse(fields[8], CultureInfo.InvariantCulture);
            float attackPeriod = float.Parse(fields[9], CultureInfo.InvariantCulture);
            bool mechanicalDamage = fields[10] == "yes";
            //field[11] isn't important
            float maxSpeedLand = float.Parse(fields[12], CultureInfo.InvariantCulture);
            float maxSpeedWater = float.Parse(fields[13], CultureInfo.InvariantCulture);
            Movement movement = (Movement)Enum.Parse(typeof(Movement), fields[14]);
            bool thickSkin = fields[15] == "yes";
            Diet diet = (Diet)Enum.Parse(typeof(Diet), fields[16]);
            float spawningTime = float.Parse(fields[17], CultureInfo.InvariantCulture);
            List<StatusFactory> statusFactories = ParseStatuses(fields[19], statuses);
            int air = int.Parse(fields[20], CultureInfo.InvariantCulture);

            Factorys.Add(unitType, 
                new AnimalFactory(
                    unitType: unitType,
                    maxHealth: maxHealth,
                    maxEnergy: maxEnergy,
                    foodEnergyRegen: foodEnergyRegen,
                    foodEatingPeriod: foodEatingPeriod,
                    radius: radius,
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

            abilitiesList.Add(unitType, fields[19]);
        }
    }
}
