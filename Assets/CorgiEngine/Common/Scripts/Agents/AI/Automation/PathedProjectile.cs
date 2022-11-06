using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// This class handles the movement of a pathed projectile
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Automation/Pathed Projectile")] 
	public class PathedProjectile : MonoBehaviour
	{
		[MMInformation("A GameObject with this component will move towards its target and get destroyed when it reaches it. Here you can define what object to instantiate on impact. Use the Initialize method to set its destination and speed.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the MMFeedbacks to play when the object gets destroyed
		[Tooltip("the MMFeedbacks to play when the object gets destroyed")]
		public MMFeedbacks DestroyFeedbacks;

		/// the destination of the projectile
		protected Transform _destination;
		/// the movement speed
		protected float _speed;

		/// <summary>
		/// Initializes the specified destination and speed.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="speed">Speed.</param>
		public virtual void Initialize(Transform destination, float speed)
		{
			_destination=destination;
			_speed=speed;
		}

		/// <summary>
		/// Every frame, me move the projectile's position to its destination
		/// </summary>
		protected virtual void Update () 
		{
			transform.position=Vector3.MoveTowards(transform.position,_destination.position,Time.deltaTime * _speed);
			float distanceSquared = (_destination.transform.position - transform.position).sqrMagnitude;
			if(distanceSquared > .01f * .01f)
			{
				return;
			}

			DestroyFeedbacks?.PlayFeedbacks();			
			
			Destroy(gameObject);
		}	
	}
}