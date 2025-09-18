using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
        public bool attack;
		public bool interact;
		public bool takeDamage;
		public bool fixItem;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnAttack(InputValue value)
        {
            attackInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            interactInput(value.isPressed);
        }

        public void OnTakeDamage(InputValue value)
        {
            takeDamageInput(value.isPressed);
        }

        public void OnFixItem(InputValue value)
        {
            fixItemInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

        public void attackInput(bool newAttackInput)
        {
            attack = newAttackInput;
        }

        public void interactInput(bool newAttackInput)
        {
            interact = newAttackInput;
        }

        public void takeDamageInput(bool newtakeDamageInput)
        {
            takeDamage = newtakeDamageInput;
        }

        public void fixItemInput(bool newFixItemInput)
        {
            fixItem = newFixItemInput;
        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}