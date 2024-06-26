//
// SPDX-License-Identifier: Apache-2.0
//
// The LEAP 71 ShapeKernel is an open source geometry engine
// specifically for use in Computational Engineering Models (CEM).
//
// For more information, please visit https://leap71.com/shapekernel
// 
// This project is developed and maintained by LEAP 71 - © 2023 by LEAP 71
// https://leap71.com
//
// Computational Engineering will profoundly change our physical world in the
// years ahead. Thank you for being part of the journey.
//
// We have developed this library to be used widely, for both commercial and
// non-commercial projects alike. Therefore, have released it under a permissive
// open-source license.
// 
// The LEAP 71 ShapeKernel is based on the PicoGK compact computational geometry 
// framework. See https://picogk.org for more information.
//
// LEAP 71 licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with the
// License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, THE SOFTWARE IS
// PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// See the License for the specific language governing permissions and
// limitations under the License.   
//

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;


namespace Leap71
{
    namespace ShapeKernel
    {
        //Modulation in 1D
        public class LineModulation : IDeepCloneable<LineModulation>
        {
            public delegate float       RatioFunc(float fRatio);
            public enum                 EInput { FUNC };
            public enum                 ECoord { X, Y, Z };

            protected ECoord            m_eValues;
            protected ECoord            m_eAxis;
            protected List<Vector3>     m_aDiscretePoints;
            protected List<float>       m_aDiscreteValues;
            public List<float>          m_aDiscreteLengthRatios;
            protected RatioFunc         m_oFunc;
            protected EInput            m_eInput;
            public float                m_fConstValue;


            /// <summary>
            /// Line modulation built around a constant value.
            /// </summary>
            public LineModulation(float fConstValue)
            {
                m_fConstValue           = fConstValue;
                m_oFunc                 = fConstLineDummyFunc;
                m_eInput                = EInput.FUNC;
            }

            /// <summary>
            /// Line modulation based on a (continuous) 1D function.
            /// </summary>
            public LineModulation(RatioFunc oModulationFunc)
            {
                m_oFunc                 = oModulationFunc;
                m_eInput                = EInput.FUNC;
            }

            /// <summary>
            /// Line modulation based on values at given length ratios. The two lists must be of equal length and the length ratios should be sorted in ascending order.
            /// </summary>
            /// <param name="modulationValues"></param>
            /// <param name="parameterValues"></param>
            public LineModulation(List<float> aDiscreteValues, List<float> aLengthRatios)
            {
                m_aDiscreteValues = aDiscreteValues;
                m_aDiscreteLengthRatios = aLengthRatios;

                if (aDiscreteValues.Count != aLengthRatios.Count)
                {
                    throw new Exception("Values and length ratio lists must have the same length.");
                }

                m_oFunc = oValuesDummyFunc;
            }

            public LineModulation DeepCopy()
            {
                return (LineModulation)MemberwiseClone();
            }

            protected float oValuesDummyFunc(float fRatio)
            {
                int closestIndex = FindClosestIndex(fRatio);

                float x0 = m_aDiscreteLengthRatios[closestIndex];
                float x1 = m_aDiscreteLengthRatios[closestIndex + 1];
                float y0 = m_aDiscreteValues[closestIndex];
                float y1 = m_aDiscreteValues[closestIndex + 1];

                float output = y0 + (fRatio - x0) * (y1 - y0) / (x1 - x0);

                if (output > m_aDiscreteValues.Max())
                {
                    throw new Exception("Something went terribly wrong...");
                }

                return output;
            }

            protected int FindClosestIndex(float fRatio)
            {
                int closestIndex = 0;

                if (fRatio <= 0)
                {
                    return 0;
                }

                if (fRatio >= 1)
                {
                    return m_aDiscreteLengthRatios.Count - 2;
                }

                else
                {
                    for (int i = 0; i < m_aDiscreteLengthRatios.Count - 1; i++)
                    {
                        if (fRatio >= m_aDiscreteLengthRatios[i] && fRatio <= m_aDiscreteLengthRatios[i + 1])
                        {
                            closestIndex = i;
                            break;
                        }
                    }
                }
                return closestIndex;
            }

            /// <summary>
            /// Line modulation based on a discrete distribution.
            /// </summary>
            public LineModulation(List<Vector3> aDiscretePoints, ECoord eValues, ECoord eAxis)
            {
                m_eValues               = eValues;
                m_eAxis                 = eAxis;
                m_aDiscretePoints       = aDiscretePoints;

                //extend list, so that last value is found and does not jump back to start value
                Vector3 vecValueUnit    = Vector3.UnitX;
                float fLastValue        = m_aDiscretePoints[m_aDiscretePoints.Count].X;
                if (m_eValues == ECoord.Y)
                {
                    vecValueUnit        = Vector3.UnitY;
                    fLastValue          = m_aDiscretePoints[m_aDiscretePoints.Count].Y;
                }
                else if (m_eValues == ECoord.Z)
                {
                    vecValueUnit        = Vector3.UnitZ;
                    fLastValue          = m_aDiscretePoints[m_aDiscretePoints.Count].Z;
                }

                Vector3 vecAxisUnit = Vector3.UnitX;
                if (m_eAxis == ECoord.Y)
                {
                    vecAxisUnit = Vector3.UnitY;
                }
                else if (m_eAxis == ECoord.Z)
                {
                    vecAxisUnit = Vector3.UnitZ;
                }

                m_oFunc             = oPointsDummyFunc;
                m_eInput            = EInput.FUNC;

                Vector3 vecLast     = fLastValue * vecValueUnit + 1.01f * vecAxisUnit;
                Vector3 vecVeryLast = fLastValue * vecValueUnit + 1.1f * vecAxisUnit;
                m_aDiscretePoints.Add(vecLast);
                m_aDiscretePoints.Add(vecVeryLast);

            }

            protected float fConstLineDummyFunc(float fRatio)
            {
                return m_fConstValue;
            }

            protected float oPointsDummyFunc(float fRatio)
            {
                //initialise for the first point
                float dS            = 0;
                float fLowerRatio   = 0;
                float fUpperRatio   = 1f;
                int iLowerIndex     = 0;

                for (int i = 0; i < m_aDiscretePoints.Count - 2; i++)
                {
                    float fCurrentRatio = 0;
                    float fNextRatio    = 0;
                    if (m_eAxis == ECoord.X)
                    {
                        fCurrentRatio   = m_aDiscretePoints[i].X;
                        fNextRatio      = m_aDiscretePoints[i + 1].X;
                    }
                    else if (m_eAxis == ECoord.Y)
                    {
                        fCurrentRatio   = m_aDiscretePoints[i].Y;
                        fNextRatio      = m_aDiscretePoints[i + 1].Y;
                    }
                    else if (m_eAxis == ECoord.Z)
                    {
                        fCurrentRatio   = m_aDiscretePoints[i].Z;
                        fNextRatio      = m_aDiscretePoints[i + 1].Z;
                    }
                    if (fCurrentRatio >= fRatio)
                    {
                        fLowerRatio     = fCurrentRatio;
                        dS              = fRatio - fLowerRatio;
                        fUpperRatio     = fNextRatio;
                        iLowerIndex     = i;
                        break;
                    }
                }

                //restrict
                iLowerIndex = Math.Max(0, Math.Min(iLowerIndex, m_aDiscretePoints.Count - 2));
                int iUpperIndex         = (int)(iLowerIndex + 1);
                float dRatio            = fUpperRatio - fLowerRatio;

                float fValue = 0;
                if (m_eValues == ECoord.X)
                {
                    fValue = (m_aDiscretePoints[iUpperIndex].X - m_aDiscretePoints[iLowerIndex].X) / dRatio * dS + m_aDiscretePoints[iLowerIndex].X;
                }
                else if (m_eValues == ECoord.Y)
                {
                    fValue = (m_aDiscretePoints[iUpperIndex].Y - m_aDiscretePoints[iLowerIndex].Y) / dRatio * dS + m_aDiscretePoints[iLowerIndex].Y;
                }
                else if (m_eValues == ECoord.Z)
                {
                    fValue = (m_aDiscretePoints[iUpperIndex].Z - m_aDiscretePoints[iLowerIndex].Z) / dRatio * dS + m_aDiscretePoints[iLowerIndex].Z;
                }
                return fValue;
            }

            protected Vector3 vecFromDim(ECoord eCoord)
            {
                if (eCoord == ECoord.Y)
                {
                    return Vector3.UnitY;
                }
                else if (eCoord == ECoord.Z)
                {
                    return Vector3.UnitZ;
                }
                else
                {
                    return Vector3.UnitX;
                }
            }

            /// <summary>
            /// Returns the underlaying input type for this modulation.
            /// </summary>
            public EInput oGetInputType()
            {
                return m_eInput;
            }

            /// <summary>
            /// Queries the value of the line modulation at a given ratio.
            /// The ratio should be between 0 and 1.
            /// </summary>
            public float fGetModulation(float fRatio)
            {
                return m_oFunc(fRatio);
            }

            protected static LineModulation m_oMod1, m_oMod2;
            public static LineModulation operator +(LineModulation oMod1, LineModulation oMod2)
            {
                m_oMod1 = oMod1;
                m_oMod2 = oMod2;
                return new LineModulation(fGetAddedModulation);
            }

            protected static float fGetAddedModulation(float fLengthRatio)
            {
                return m_oMod1.fGetModulation(fLengthRatio) + m_oMod2.fGetModulation(fLengthRatio);
            }

            public static LineModulation operator -(LineModulation oMod1, LineModulation oMod2)
            {
                m_oMod1 = oMod1;
                m_oMod2 = oMod2;
                return new LineModulation(fGetSubtractedModulation);
            }

            protected static float fGetSubtractedModulation(float fLengthRatio)
            {
                return m_oMod1.fGetModulation(fLengthRatio) - m_oMod2.fGetModulation(fLengthRatio);
            }

            public LineModulation DeepClone()
            {
                LineModulation clone = (LineModulation)MemberwiseClone();

                // Reference type clones
                clone.m_aDiscretePoints = m_aDiscretePoints != null ? new List<Vector3>(m_aDiscretePoints) : null;
                clone.m_aDiscreteValues = m_aDiscreteValues != null ? new List<float>(m_aDiscreteValues) : null;
                clone.m_aDiscreteLengthRatios = m_aDiscreteLengthRatios !=  null ? new List<float>(m_aDiscreteLengthRatios) : null;

                // Delegate clone
                // Revise this
                clone.m_oFunc = m_oFunc;

                return clone;
            }
        }
    }
}