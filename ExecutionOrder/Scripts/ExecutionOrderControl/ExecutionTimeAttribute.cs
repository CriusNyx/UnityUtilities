﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities.ExecutionOrder.ExecutionOrderControl
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ExecutionOrderAttribute : Attribute
    {
        public readonly int order;

        public ExecutionOrderAttribute(ExecutionOrderValue order)
        {
            this.order = (int)order;
        }

        public ExecutionOrderAttribute(int order)
        {
            this.order = order;
        }
    }

    public enum ExecutionOrderValue
    {
        Earliest = -20,
        Early = -10,
        PreLogic = -1,
        Logic = 0,
        PostLogic = 1,
        PrePhysics = 9,
        Physics = 10,
        PrePhysicsStep = 11,
        PhysicsStep = 12,
        PostPhysics = 13,
        Camera = 20,
        PostCamera = 21,
    }
}