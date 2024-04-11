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

using System.Numerics;
using PicoGK;

namespace Leap71.ShapeKernel
{
    public class ImplicitSphere : ImplicitBaseShape, IImplicit
    {
        readonly Vector3 _centrePoint;
        readonly float _radius;

        public ImplicitSphere(Vector3 centrePoint, float radius)
        {
            _centrePoint = centrePoint;
            _radius = radius;
        }

        public override float fSignedDistance(in Vector3 vec)
        {
            return Vector3.Distance(vec, _centrePoint) - _radius;
        }

        protected override BBox3 BoundingBox()
        {
            return new BBox3(
                _centrePoint - new Vector3(_radius),
                _centrePoint + new Vector3(_radius)
                );
        }
    }
}
