using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Gives health to the player who collects it
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Stimpack")]
	public class Stimpack : PickableItem
	{
		/// the amount of health to give the player when collected
		[Tooltip("the amount of health to give the player when collected")]
		public float HealthToGive = 10f;

		/// <summary>
		/// What happens when the object gets picked
		/// </summary>
		protected override void Pick(GameObject picker)
		{
			Health characterHealth = _pickingCollider.GetComponent<Health>();
			// else, we give health to the player
			characterHealth.GetHealth(HealthToGive,gameObject);
		}
	}
}