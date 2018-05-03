using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SceneObjectTransform
    {
        SceneObject _obj;
        Matrix4F _globalTransform = Matrix4F.Identity;
        Vector3F _globalPosition;

        Matrix4F _localTransform = Matrix4F.Identity;
        Vector3F _localPosition;
        Vector3F _localScale = Vector3F.One;
        Vector3F _localUp = Vector3F.Up;

        Vector3F _angles;
        QuaternionF _orientation = QuaternionF.Identity;

        bool _globalChanged = true;
        bool _localChanged = true;

        internal SceneObjectTransform(SceneObject obj)
        {
            _obj = obj;
        }

        internal void CalculateLocal()
        {
            _orientation = QuaternionF.RotationYawPitchRoll(_angles.Y * MathHelper.DegToRad, 
                _angles.X * MathHelper.DegToRad,
                _angles.Z * MathHelper.DegToRad);
            _localTransform = Matrix4F.Scaling(_localScale) * Matrix4F.FromQuaternion(_orientation) * Matrix4F.CreateTranslation(_localPosition);
        }

        /// <summary>Calculate the global transform at the exact same position as the global one, relative to the ex-parent.</summary>
        internal void Detach(SceneObjectTransform exParent)
        {
            _localTransform = Matrix4F.Invert(exParent._globalTransform) * _globalTransform;
            _localChanged = true;
        }

        /// <summary>Calculate the local transform at the exact same position as the global one, relative to the new parent.</summary>
        /// <param name="parent"></param>
        internal void Attach(SceneObjectTransform newParent)
        {
            // Subtract the parent global from the current global.
        }

        internal void CalculateGlobal(SceneObjectTransform parent)
        {
            _globalTransform = _localTransform * parent._globalTransform;
            _globalPosition = _globalTransform.Translation;
        }

        /// <summary>Calculates a non-parented global transform.</summary>
        internal void CalculateGlobal()
        {
            _globalPosition = _localPosition;
            _globalTransform = _localTransform;
        }

        internal void Update(Timing time)
        {
            if (_localChanged)
                CalculateLocal();

            // Calculate global 
            if (_obj.Parent != null)
            {
                if (_obj.Parent.Transform._globalChanged)
                    CalculateGlobal(_obj.Parent.Transform);
            }
            else
            {
                if (_localChanged)
                {
                    _globalChanged = true;
                    CalculateGlobal();
                }
            }
        }

        internal void ResetFlags()
        {
            _globalChanged = false;
            _localChanged = false;
        }

        public Matrix4F Global => _globalTransform;

        public Vector3F GlobalPosition => _globalPosition;

        public Matrix4F Local => _localTransform;

        public Vector3F LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                _localChanged = true;
            }
        }

        /// <summary>Gets or sets the rotation around the X axis, in degrees. </summary>
        public float LocalRotationX
        {
            get => _angles.X;
            set
            {
                _angles.X = value;
                _localChanged = true;
            }
        }

        /// <summary>Gets or sets the rotation around the Y axis, in degrees. </summary>
        public float LocalRotationY
        {
            get => _angles.Y;
            set
            {
                _angles.Y = value;
                _localChanged = true;
            }
        }

        /// <summary>Gets or sets the rotation around the Z axis, in degrees. </summary>
        public float LocalRotationZ
        {
            get => _angles.Z;
            set
            {
                _angles.Z = value;
                _localChanged = true;
            }
        }

        /// <summary>Gets or sets the scale along the X axis, in degrees. </summary>
        public float LocalScaleX
        {
            get => _localScale.X;
            set
            {
                _localScale.X = value;
                _localChanged = true;
            }
        }

        /// <summary>Gets or sets the scale along the Y axis, in degrees. </summary>
        public float LocalScaleY
        {
            get => _localScale.Y;
            set
            {
                _localScale.Y = value;
                _localChanged = true;
            }
        }

        /// <summary>Gets or sets the scale along the Z axis, in degrees. </summary>
        public float LocalScaleZ
        {
            get => _localScale.Z;
            set
            {
                _localScale.Z = value;
                _localChanged = true;
            }
        }

        public Vector3F LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value;
                _localChanged = true;
            }
        }
    }
}
