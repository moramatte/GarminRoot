//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Logging Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;

namespace Infrastructure.Logger.Enterprise
{
    /// <summary>
    /// A data time provider.
    /// </summary>
    public class DateTimeProvider
	{
		/// <summary>
		/// Gets the current data time.
		/// </summary>
		/// <value>
		/// The current data time.
		/// </value>
		public virtual DateTime CurrentUtcDateTime
		{
			get { return DateTime.UtcNow; }
		}
	}
}
