﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Remove leading and trailing whitespace from a string, and return the updated string.
    /// </summary>
    public class Trim : StringTransformEvaluator
    {
        public Trim()
            : base(ExpressionType.Trim, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            if (args[0] == null)
            {
                return string.Empty;
            }
            else
            {
                return args[0].ToString().Trim();
            }
        }
    }
}
