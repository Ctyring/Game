﻿using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This decision will return true when entering the state this Decision is on.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Decisions/AI Decision Next Frame")]
	public class AIDecisionNextFrame : AIDecision
	{
		/// <summary>
		/// We return true on Decide
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return true;
		}
	}
}