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
using PicoGK;

namespace Leap71
{
    namespace ShapeKernel
    {
        public abstract class ImplicitBaseShape : IImplicit
        {
            Voxels _voxels;
            Mesh _mesh;
            BBox3 _bbox;

            public BBox3 BBox
            {
                get
                {
                    return BoundingBox();
                }
            }

            public Voxels Voxels
            {
                get
                {
                    return new Voxels(this, BBox); ;
                }
            }

            public Mesh Mesh
            {
                get
                {
                    if ( _mesh == null)
                    {
                        _mesh = new Mesh(Voxels);
                    }
                    return _mesh;
                }
            }

            public abstract float fSignedDistance(in Vector3 vec);

            protected abstract BBox3 BoundingBox();
        }

    }
}
