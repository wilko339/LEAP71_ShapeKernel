// Copyright 2024 Toby Wilkinson
//
//  Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0 
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//  See the License for the specific language governing permissions and 
//  limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Leap71.ShapeKernel;
using PicoGK;

namespace Leap71.ShapeKernel
{
    public class ImplicitGeneric : ImplicitBaseShape, IImplicit
    {
        Func<float, float, float, float> _sdf;
        BBox3 _boundingBox;

        public ImplicitGeneric(Func<float, float, float, float> sdf, BBox3 boundingBox)
        {
            _sdf = sdf;
            _boundingBox = boundingBox;
        }
        public override float fSignedDistance(in Vector3 vec)
        {
            return _sdf(vec.X, vec.Y, vec.Z);
        }

        protected override BBox3 BoundingBox()
        {
            return _boundingBox;
        }
    }
}
