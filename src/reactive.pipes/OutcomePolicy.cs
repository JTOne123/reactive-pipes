﻿// Copyright (c) Daniel Crenna. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace reactive.pipes
{
	public enum OutcomePolicy
	{
		/// <summary>
		/// All handlers must report success to consider the outcome satisfied.
		/// </summary>
		Pessimistic,

		/// <summary>
		/// Any handler can report success to consider the outcome satisfied.
		/// </summary>
		Optimistic
	}
}