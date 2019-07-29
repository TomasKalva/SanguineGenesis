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
        private Dictionary<Key, Ability> keyToAbility;
        public UnitsCommandInputState State { get; set; }
        public Vector2 MapCoordinates { get; private set; }
        public Entity TargetedUnit { get; private set; }
        private Ability selectedAbility;
        public Ability SelectedAbility { get { return selectedAbility; } set { selectedAbility = value; AbilitySelected = true; } }
        public bool AbilitySelected { get; set; }
        public Ability Ability => SelectedAbility;// AbilityTypeToAbility[SelectedAbility];

        private static UnitCommandsInput unitCommandsInput=new UnitCommandsInput();
        public static UnitCommandsInput GetUnitCommandsInput() => unitCommandsInput;

        private UnitCommandsInput()
        {
            State = UnitsCommandInputState.IDLE;
            MapCoordinates = new Vector2();
            SelectedAbility = MoveToPoint.Get;
            AbilitySelected = false;

            //initialize keyToAbilityType
            keyToAbility = new Dictionary<Key, Ability>();
            keyToAbility.Add(Key.Escape, MoveToPoint.Get);

            //initialize abilityTypeToAbility
            /*AbilityTypeToAbility = new Dictionary<Ability, Ability>();
            AbilityTypeToAbility.Add(SelectedAbility.MOVE_TO,
                new TargetPointAbility(SelectedAbility.MOVE_TO, 0.1f));
            AbilityTypeToAbility.Add(SelectedAbility.ATTACK,
                 new TargetUnitAbility(SelectedAbility.ATTACK, 1.2f, true));*/
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
