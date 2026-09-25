using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Services
{
	public class KeyBindingsData : ISettingsData
	{
		public static readonly IReadOnlyDictionary<ControlAction, KeyCode> Defaults = new Dictionary<ControlAction, KeyCode>
		{
			[ControlAction.MoveForward] = KeyCode.W,
			[ControlAction.MoveBackward] = KeyCode.S,
			[ControlAction.MoveLeft] = KeyCode.A,
			[ControlAction.MoveRight] = KeyCode.D,
			[ControlAction.Jump] = KeyCode.Space,
			[ControlAction.Sprint] = KeyCode.LeftShift,
			[ControlAction.PrimaryAction] = KeyCode.Mouse0,
			[ControlAction.SecondaryAction] = KeyCode.Mouse1,
			[ControlAction.Reload] = KeyCode.R,
			[ControlAction.RotateBlueprint] = KeyCode.R,
			[ControlAction.BlueprintMenu] = KeyCode.E,
			[ControlAction.Palette] = KeyCode.Q,
			[ControlAction.PaletteUp] = KeyCode.UpArrow,
			[ControlAction.PaletteDown] = KeyCode.DownArrow,
			[ControlAction.PaletteLeft] = KeyCode.LeftArrow,
			[ControlAction.PaletteRight] = KeyCode.RightArrow,
			[ControlAction.PickColor] = KeyCode.Mouse2,
			[ControlAction.Scoreboard] = KeyCode.Tab,
			[ControlAction.ChooseClass] = KeyCode.N,
			[ControlAction.Map] = KeyCode.M,
			[ControlAction.Slot1] = KeyCode.Alpha1,
			[ControlAction.Slot2] = KeyCode.Alpha2,
			[ControlAction.Slot3] = KeyCode.Alpha3,
			[ControlAction.Slot4] = KeyCode.Alpha4,
			[ControlAction.Slot5] = KeyCode.Alpha5,
			[ControlAction.Slot6] = KeyCode.Alpha6,
			[ControlAction.Slot7] = KeyCode.Alpha7,
			[ControlAction.Slot8] = KeyCode.Alpha8,
			[ControlAction.Slot9] = KeyCode.Alpha9,
			[ControlAction.Slot10] = KeyCode.Alpha0,
		};

		public readonly Dictionary<ControlAction, KeyCode> Bindings;

		[JsonConstructor]
		public KeyBindingsData(Dictionary<ControlAction, KeyCode> bindings)
		{
			Bindings = new Dictionary<ControlAction, KeyCode>(Defaults);

			// Actions added after the file was saved keep their default keys.
			if (bindings != null)
			{
				foreach ((ControlAction action, KeyCode key) in bindings)
				{
					Bindings[action] = key;
				}
			}
		}

		public KeyBindingsData()
			: this(null)
		{
		}

		public KeyCode GetKey(ControlAction action)
		{
			return Bindings.TryGetValue(action, out KeyCode key) ? key : Defaults[action];
		}

		// Assigns the key; an action that already used it takes over the previous key of this one.
		public KeyBindingsData WithBinding(ControlAction action, KeyCode key)
		{
			var bindings = new Dictionary<ControlAction, KeyCode>(Bindings);
			KeyCode previousKey = GetKey(action);

			foreach ((ControlAction otherAction, KeyCode otherKey) in Bindings)
			{
				if (otherAction != action && otherKey == key)
				{
					bindings[otherAction] = previousKey;
				}
			}

			bindings[action] = key;
			return new KeyBindingsData(bindings);
		}
	}
}
