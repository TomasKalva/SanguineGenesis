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
        public Dictionary<string, Factory> FactoryMap { get; }
        /// <summary>
        /// For each entity name contains list of ability names separated by ';' that
        /// this entity factory will parse.
        /// </summary>
        protected Dictionary<string, string> abilitiesList;

        public Factories()
        {
            FactoryMap = new Dictionary<string, Factory>();
            abilitiesList = new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns factory for the entity type.
        /// </summary>
        public Factory this[string entityType] 
        {
            get
            {
                if (FactoryMap.TryGetValue(entityType, out Factory factory))
                    return factory;
                else
                    throw new ArgumentException($"There is no {typeof(Factory)} for {entityType}");
            }
        }

        /// <summary>
        /// Load FactoryMap and abilities from the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="statuses">Statuses that can be given to factories that require them.</param>
        /// <exception cref="ArgumentException">Thrown if input file has incorrect format.</exception>
        public void InitFactoryMap(string fileName, Statuses.Statuses statuses)
        {
            using (StreamReader fileReader = new StreamReader(fileName))
            {
                //starting at postion 2, postion 1 is header
                int lineN = 2;
                try
                {
                    //first line is just a description of the format
                    string line = fileReader.ReadLine();
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        AddNewFactory(line, statuses);
                        lineN++;
                    }
                }catch(Exception e) when (e is ArgumentException || e is IOException || e is OutOfMemoryException)
                {
                    throw new ArgumentException($"Error in file {fileName} at line {lineN}: {e.Message}", e);
                }
            }
        }

        /// <summary>
        /// Adds a new factory to the FactoryMap.
        /// </summary>
        /// <param name="description">String that describes the factory's properties.</param>
        /// <param name="statuses">Statuses that can be given to the factory if it requires them.</param>
        /// <exception cref="ArgumentException">Thrown when some argument in description has incorrect value.</exception>
        public abstract void AddNewFactory(string description, Statuses.Statuses statuses);

        /// <summary>
        /// Returns a list of statuses represented by the string listOfStatuses.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if some argument in listOfStatuses has incorrect value.</exception>
        public List<StatusFactory> ParseStatuses(string listOfStatuses, Statuses.Statuses statuses)
        {
            List<StatusFactory> statusFactories = new List<StatusFactory>();
            if (listOfStatuses == "")
                return statusFactories;

            string[] statusesNames = listOfStatuses.Split(';');
            foreach(string stName in statusesNames)
            {
                switch (stName)
                {
                    case "holeSystem":
                        statusFactories.Add(statuses.HoleSystem);
                        break;
                    default:
                        throw new ArgumentException($"Status {stName} doesn't exist.");
                }
            }
            return statusFactories;
        }

        /// <summary>
        /// Uses abilitiesList to set abilities to the already created FactoryMap. Has to be called after
        /// InitFactoryMap.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if some argument in abilities has incorrect value.</exception>
        public void InitAbilities(Abilities.Abilities abilities)
        {
            foreach(var entityAbilities in abilitiesList)
            {
                Factory factory = this[entityAbilities.Key];
                string[] abilitiesDesc = entityAbilities.Value.Split(';');
                foreach(string abilityDesc in abilitiesDesc)
                {
                    //skip empty strings
                    if (abilityDesc == "")
                        continue;

                    //parameters of ability are separated by ':'
                    string[] abPar = abilityDesc.Split(':');
                    string abName = abPar[0];
                    if (abPar.Length == 1)
                    {
                        //abilities with no parameters
                        switch (abName)
                        {
                            case "unbreakableMoveTo":
                                 factory.AddAbility(abilities.UnbreakableMoveTo);
                                break;
                            case "moveTo":
                                factory.AddAbility(abilities.MoveTo);
                                break;
                            case "unbreakableAttack":
                                factory.AddAbility(abilities.UnbreakableAttack);
                                break;
                            case "attack":
                                factory.AddAbility(abilities.Attack);
                                break;
                            case "rallyPoint":
                                factory.AddAbility(abilities.SetRallyPoint);
                                break;
                            case "eat":
                                //check if animal is herbivore or carnivore
                                if (factory is AnimalFactory animF)
                                {
                                    if (animF.Diet == Diet.HERBIVORE)
                                        factory.AddAbility(abilities.HerbivoreEat);
                                    else
                                        factory.AddAbility(abilities.CarnivoreEat);
                                }
                                else
                                {
                                    throw new ArgumentException("Ability eat can be only used by animals.");
                                }
                                break;
                            case "poisonousSpit":
                                factory.AddAbility(abilities.PoisonousSpit);
                                break;
                            case "activateSprint":
                                factory.AddAbility(abilities.ActivateSprint);
                                break;
                            case "piercingBite":
                                factory.AddAbility(abilities.PiercingBite);
                                break;
                            case "consumeAnimal":
                                factory.AddAbility(abilities.ConsumeAnimal);
                                break;
                            case "jump":
                                factory.AddAbility(abilities.Jump);
                                break;
                            case "activateShell":
                                factory.AddAbility(abilities.ActivateShell);
                                break;
                            case "pull":
                                factory.AddAbility(abilities.Pull);
                                break;
                            case "bigPull":
                                factory.AddAbility(abilities.BigPull);
                                break;
                            case "farSight":
                                factory.AddAbility(abilities.ActivateFarSight);
                                break;
                            case "knockBack":
                                factory.AddAbility(abilities.KnockBack);
                                break;
                            case "climbPlant":
                                factory.AddAbility(abilities.ClimbPlant);
                                break;
                            case "enterHole":
                                factory.AddAbility(abilities.EnterHole);
                                break;
                            case "exitHole":
                                factory.AddAbility(abilities.ExitHole);
                                break;
                            case "fastStrikes":
                                factory.AddAbility(abilities.ActivateFastStrikes);
                                break;
                            case "improveStructure":
                                factory.AddAbility(abilities.ImproveStructure);
                                break;
                            case "chargeTo":
                                factory.AddAbility(abilities.ChargeTo);
                                break;
                            case "kick":
                                factory.AddAbility(abilities.Kick);
                                break;
                            default:
                                throw new ArgumentException($"Ability {abName} doesn't exist.");
                        }
                    }else if (abPar.Length == 2)
                    {
                        //abilities with one parameter
                        switch (abName)
                        {
                            case "build":
                                factory.AddAbility(abilities.BuildBuilding(abPar[1]));
                                break;
                            case "spawn":
                                factory.AddAbility(abilities.AnimalSpawn(abPar[1]));
                                break;
                            case "create":
                                factory.AddAbility(abilities.AnimalCreate(abPar[1]));
                                break;
                            default:
                                throw new ArgumentException($"Ability {abName} with 1 parameter doesn't exist.");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Loads and stores factories of the type PlantFactory.
    /// </summary>
    class PlantFactories : Factories<PlantFactory>
    {
        public override void AddNewFactory(string description, Statuses.Statuses statuses)
        {
            //current parsing position
            int pos = 0;
            string[] fields = description.Split(',');
            try
            {
                string plantType = fields[0]; pos++;
                float maxHealth = float.Parse(fields[1], CultureInfo.InvariantCulture); pos++;
                float maxEnergy = float.Parse(fields[2], CultureInfo.InvariantCulture); pos++;
                float energyRegen = float.Parse(fields[3], CultureInfo.InvariantCulture); pos++;
                bool physical = fields[4] == "yes"; pos++;
                int size = int.Parse(fields[5], CultureInfo.InvariantCulture); pos++;
                int rootsDistance = int.Parse(fields[6], CultureInfo.InvariantCulture); pos++;
                float energyCost = float.Parse(fields[7], CultureInfo.InvariantCulture); pos++;
                Biome biome = (Biome)Enum.Parse(typeof(Biome), fields[8]); pos++;
                Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain), fields[9]); pos++;
                SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality), fields[10]); pos++; pos++;
                List<StatusFactory> statusFactories = ParseStatuses(fields[12], statuses); pos++;
                bool producer = fields[13] == "yes"; pos++;
                int air = int.Parse(fields[14], CultureInfo.InvariantCulture); pos++;
                float buildingDistance = float.Parse(fields[15], CultureInfo.InvariantCulture); pos++;
                float viewRange = float.Parse(fields[16], CultureInfo.InvariantCulture); pos++;
                bool blocksVision = fields[17] == "yes"; pos++;
                pos=0;

                PlantFactory newFactory = new PlantFactory(plantType, maxHealth, maxEnergy, energyRegen, size, physical, energyCost,
                    biome, terrain, soilQuality, producer, buildingDistance, viewRange, blocksVision, rootsDistance, air, statusFactories);
                FactoryMap.Add(plantType, newFactory);
                abilitiesList.Add(plantType, fields[11]);
            }
            catch(Exception e) when (e is ArgumentException || e is OverflowException || e is FormatException)
            {
                throw new ArgumentException($"Invalid value '{fields[pos]}' at position {pos+1}.");
            }
        }
    }

    /// <summary>
    /// Loads and stores factories of the type StructureFactory.
    /// </summary>
    class StructureFactories : Factories<StructureFactory>
    {
        public override void AddNewFactory(string description, Statuses.Statuses statuses)
        {
            //current parsing position
            int pos = 0;
            string[] fields = description.Split(',');
            try
            {
                string structureType = fields[0]; pos++;
                float maxHealth = float.Parse(fields[1], CultureInfo.InvariantCulture); pos++;
                float maxEnergy = float.Parse(fields[2], CultureInfo.InvariantCulture); pos++;
                bool physical = fields[3] == "yes"; pos++;
                int size = int.Parse(fields[4], CultureInfo.InvariantCulture); pos++;
                float energyCost = float.Parse(fields[5], CultureInfo.InvariantCulture); pos++;
                Biome biome = (Biome)Enum.Parse(typeof(Biome), fields[6]); pos++;
                Terrain terrain = (Terrain)Enum.Parse(typeof(Terrain), fields[7]); pos++;
                SoilQuality soilQuality = (SoilQuality)Enum.Parse(typeof(SoilQuality), fields[8]); pos++; pos++;
                List<StatusFactory> statusFactories = ParseStatuses(fields[10], statuses); pos++;
                float buildingDistance = float.Parse(fields[11], CultureInfo.InvariantCulture); pos++;
                float viewRange = float.Parse(fields[12], CultureInfo.InvariantCulture); pos++;
                bool blocksVision = fields[13] == "yes"; pos++;
                pos = 0;

                StructureFactory newFactory = new StructureFactory(structureType, maxHealth, maxEnergy, size, physical, energyCost,
                    biome, terrain, soilQuality, buildingDistance, viewRange, blocksVision, statusFactories);
                FactoryMap.Add(structureType, newFactory);
                abilitiesList.Add(structureType, fields[9]);
            }
            catch (Exception e) when (e is ArgumentException || e is OverflowException || e is FormatException)
            {
                throw new ArgumentException($"Invalid value '{fields[pos]}' at position {pos + 1}.");
            }
        }
    }

    /// <summary>
    /// Loads and stores factories of the type AnimalFactory.
    /// </summary>
    class AnimalFactories : Factories<AnimalFactory>
    {
        public override void AddNewFactory(string description, Statuses.Statuses statuses)
        {
            //current parsing position
            int pos = 0;
            string[] fields = description.Split(',');
            try
            {
                string unitType = fields[0]; pos++;
                float maxHealth = float.Parse(fields[1], CultureInfo.InvariantCulture); pos++;
                float maxEnergy = float.Parse(fields[2], CultureInfo.InvariantCulture); pos++;
                float foodEnergyRegen = float.Parse(fields[3], CultureInfo.InvariantCulture); pos++;
                float foodEatingPeriod = float.Parse(fields[4], CultureInfo.InvariantCulture); pos++;
                float radius = float.Parse(fields[5], CultureInfo.InvariantCulture);
                if (radius > 0.5f) throw new ArgumentException("The radius of animal can be at most 0.5."); pos++;
                float energyCost = float.Parse(fields[6], CultureInfo.InvariantCulture); pos++;
                float attackDamage = float.Parse(fields[7], CultureInfo.InvariantCulture); pos++;
                float attackDistance = float.Parse(fields[8], CultureInfo.InvariantCulture); pos++;
                float attackPeriod = float.Parse(fields[9], CultureInfo.InvariantCulture); pos++;
                bool mechanicalDamage = fields[10] == "yes"; pos++;
                //field[11] isn't important - it shows damage per second
                float maxSpeedLand = float.Parse(fields[12], CultureInfo.InvariantCulture); pos++;
                float maxSpeedWater = float.Parse(fields[13], CultureInfo.InvariantCulture); pos++;
                Movement movement = (Movement)Enum.Parse(typeof(Movement), fields[14]); pos++;
                bool thickSkin = fields[15] == "yes"; pos++;
                Diet diet = (Diet)Enum.Parse(typeof(Diet), fields[16]); pos++;
                float spawningTime = float.Parse(fields[17], CultureInfo.InvariantCulture); pos++;
                List<StatusFactory> statusFactories = ParseStatuses(fields[18], statuses); pos++; pos++;
                int air = int.Parse(fields[20], CultureInfo.InvariantCulture); pos++;
                pos = 0;

                FactoryMap.Add(unitType, 
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
            catch (Exception e) when (e is ArgumentException || e is OverflowException || e is FormatException)
            {
                throw new ArgumentException($"Invalid value '{fields[pos]}' at position {pos + 1}: {e.Message}", e);
            }
        }
    }
}
