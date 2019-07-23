using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using wpfTest.GameLogic;

namespace wpfTest
{
    class UnitCommandsInput
    {
        private Dictionary<Key, AbilityType> keyToAbilityType;
        public Dictionary<AbilityType, Ability> AbilityTypeToAbility { get; }
        public UnitsCommandInputState State { get; set; }
        public Vector2 MapCoordinates { get; private set; }
        public Entity TargetedUnit { get; private set; }
        private AbilityType abilityType;
        public AbilityType AbilityType { get { return abilityType; } set { abilityType = value; AbilitySelected = true; } }
        public bool AbilitySelected { get; set; }
        public Ability Ability => AbilityTypeToAbility[AbilityType];

        private static UnitCommandsInput unitCommandsInput=new UnitCommandsInput();
        public static UnitCommandsInput GetUnitCommandsInput() => unitCommandsInput;

        private UnitCommandsInput()
        {
            State = UnitsCommandInputState.IDLE;
            MapCoordinates = new Vector2();
            AbilityType = AbilityType.MOVE_TO;
            AbilitySelected = false;

            //initialize keyToAbilityType
            keyToAbilityType = new Dictionary<Key, AbilityType>();
            keyToAbilityType.Add(Key.Escape, AbilityType.MOVE_TO);

            //initialize abilityTypeToAbility
            AbilityTypeToAbility = new Dictionary<AbilityType, Ability>();
            AbilityTypeToAbility.Add(AbilityType.MOVE_TO,
                new TargetPointAbility(AbilityType.MOVE_TO, 0.1f));
            AbilityTypeToAbility.Add(AbilityType.ATTACK,
                 new TargetUnitAbility(AbilityType.ATTACK, 1.2f, true));
        }

        public void NewPoint(Vector2 mousePos)
        {
            State = UnitsCommandInputState.SELECTING;
            MapCoordinates = mousePos;
        }

        public void EndSelection(Vector2 mousePos)
        {
            State = UnitsCommandInputState.SELECTED;
            MapCoordinates = mousePos;
        }

        public void SetTarget(Vector2 mousePos)
        {
            if(State==UnitsCommandInputState.SELECTED)
            {
                MapCoordinates = mousePos;
                State = UnitsCommandInputState.ABILITY;
            }
        }
    }

    public enum UnitsCommandInputState
    {
        IDLE,
        SELECTING,
        SELECTED,
        ABILITY
    }
}
