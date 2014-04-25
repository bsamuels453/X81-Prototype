using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MParticles;
using MParticles.MGObjects;
using MParticles.Renderers.SFML;
using SFML.Graphics;
using SFML.Window;

namespace ParticleSys
{

    public class SmokeParticleSystem : SFMLParticleSystem{
        readonly SmokeEmitter _emitter;
        public SmokeParticleSystem(Vector2 pos, Vector2 vec) {
            _emitter = new SmokeEmitter(pos, vec);

            Emitters.Add("Smoke", _emitter);
        }

        public void PauseEmission() {
            this.GetEmitter("Smoke").Enabled = false;
        }

        public void ResumeEmission() {
            this.GetEmitter("Smoke").Enabled = true;
        }

        public void SetEmitterPos(Vector2 pos){
            _emitter.SetEmitterPos(pos);
        }

        public void SetEmissionVec(Vector2 vec){
            _emitter.SetEmissionVec(vec);
        }

        public void SetEmitterMag(float mag){
            _emitter.SetEmitterMag(mag);
        }

        public void SetBaseVelocity(Vector2 vel){
            _emitter.SetBaseVelocity(vel);
        }
    
    }

    public class SmokeEmitter : AbstractEmitter{

        public void SetEmitterPos(Vector2 pos){
            _position = pos;
            EmitterProperties.Position = _position;
        }
        public void SetBaseVelocity(Vector2 vel){
            _baseVel = vel;
            ParticleProperties.VelocityMin = _baseVel + _vec * max * magnitude - new Vector2(_velVarianceX, _velVarianceY);
            ParticleProperties.VelocityMax = _baseVel + _vec * max * magnitude + new Vector2(_velVarianceX, _velVarianceY);
        }

        Vector2 _baseVel;

        public void SetEmissionVec(Vector2 vec){
            _vec = vec;
            ParticleProperties.VelocityMin = _baseVel + _vec * max * magnitude - new Vector2(_velVarianceX, _velVarianceY);
            ParticleProperties.VelocityMax = _baseVel + _vec * max * magnitude + new Vector2(_velVarianceX, _velVarianceY);
        }

        public void SetEmitterMag(float mag){
            magnitude = mag;
        }

        Vector2 _position;
        float max = 10;
        float magnitude = 1;
        Vector2 _vec;
        int _velVarianceX = 1;
        int _velVarianceY = 30;
        
        public SmokeEmitter(Vector2 pos, Vector2 vec){
            _position = pos;
            _vec = vec;

            ParticleInitialization = SmokeParticleInit;

            OnEveryUpdate += PresetOperations.UpdateAlphaBasedOnLifetimeUsingLerp;
            OnEveryUpdate += PresetOperations.UpdateScaleBasedOnLifetimeUsingLerp;
            OnEveryUpdate += PresetOperations.UpdateColorBasedOnLifetimeUsingLerp;

            OnEveryUpdate += PresetOperations.UpdatePositionAndVelocityBasedOnExternalForce;
            OnRefreshUserObject += RefreshUserObject;

            Texture texture = new Texture(@"Cloud001.png");
            Sprite sprite = new Sprite(texture) { Origin = new Vector2f(texture.Size.X / 2f, texture.Size.Y / 2f) };

            OnParticleCreated += particle => { particle.UserObject = sprite; };
            //OnNextUpdate += update;
            //OnEveryUpdate += update;
        }
        /*
        double sumtime = 0;

        void update(Particle mParticle, GameTime elapsedTime){
            sumtime += elapsedTime.ElapsedGameTime.TotalSeconds;
            var x = 50*Math.Cos(sumtime/10);
            var y = 50 * Math.Sin(sumtime/10);
            EmitterProperties.Position = new Vector2((float)x,(float) y);
            var ang = Math.Atan2(y, x);
            var mag = Math.Sqrt(x*x + y*y);
            ang = ang + 3.14159/2f;

            var xnew = Math.Cos(ang)*mag;
            var ynew = Math.Sin(ang)*mag;
            var newvel = new Vector2((float)xnew, (float)ynew);
            ParticleProperties.VelocityMin = newvel - new Vector2(_velVarianceX, _velVarianceY);
            ParticleProperties.VelocityMax = newvel + new Vector2(_velVarianceX, _velVarianceY);
        }
        */
        protected override void SetupInitialEmitterProperties() {
            base.SetupInitialEmitterProperties();

            EmitterProperties.ParticlesPerInterval = 150;
            EmitterProperties.MaxParticles = 2000;
            EmitterProperties.Position = _position;
            EmitterProperties.EmissionStyle = EmissionStyle.CONSTANT_FLOW;
        }


        protected override void SetupInitialParticleProperties() {
            ParticleProperties.LifetimeMin = 0.20f;
            ParticleProperties.LifetimeMax = 0.65f;

            ParticleProperties.InitialAlphaMin = 0.00f;
            ParticleProperties.InitialAlphaMax = 0.00f;

            ParticleProperties.MidlifeAlphaMin = 0.50f;
            ParticleProperties.MidlifeAlphaMax = 0.50f;

            ParticleProperties.FinalAlphaMin = 0.00f;
            ParticleProperties.FinalAlphaMax = 0.00f;

            ParticleProperties.InitialScaleMin = 0.05f;
            ParticleProperties.InitialScaleMax = 0.15f;

            ParticleProperties.FinalScaleMin = 0.15f;
            ParticleProperties.FinalScaleMax = 0.35f;

            ParticleProperties.InitialColorMin = new Vector3(0, 0, 0);
            ParticleProperties.InitialColorMax = new Vector3(32, 32, 32);

            ParticleProperties.MidlifeColorMin = new Vector3(64, 64, 64);
            ParticleProperties.MidlifeColorMax = new Vector3(128, 128, 128);

            ParticleProperties.FinalColorMin = new Vector3(255, 255, 255);
            ParticleProperties.FinalColorMax = new Vector3(255, 255, 255);

            ParticleProperties.VelocityMin = _vec * max * magnitude - new Vector2(_velVarianceX, _velVarianceY);
            ParticleProperties.VelocityMax = _vec * max * magnitude + new Vector2(_velVarianceX, _velVarianceY);

            ParticleProperties.ExternalForceMin = new Vector2(5, 5);
            ParticleProperties.ExternalForceMax = new Vector2(15, 15);

            ParticleProperties.PositionOffsetFromEmitterPosition = true;

            ParticleProperties.InterpolateMinMaxAcceleration = false;
            ParticleProperties.InterpolateMinMaxExternalForce = false;
            ParticleProperties.InterpolateMinMaxFinalColor = true;
            ParticleProperties.InterpolateMinMaxInitialColor = true;
            ParticleProperties.InterpolateMinMaxMidColor = true;
            ParticleProperties.InterpolateMinMaxPosition = false;
            ParticleProperties.InterpolateMinMaxVelocity = false;
        }

        public void SmokeParticleInit(Particle particle) {
            InitializeParticleUsingInitialProperties(particle);
        }

        private void RefreshUserObject(Particle particle) {
            Sprite shape = (Sprite)particle.UserObject;
            shape.Position = new Vector2f(particle.Position.X, particle.Position.Y);
            shape.Color = new Color((byte)particle.Color.X, (byte)particle.Color.Y, (byte)particle.Color.Z,
                                    (byte)(particle.Alpha * 255));
            shape.Rotation = particle.Rotation;
            shape.Scale = new Vector2f(particle.Scale, particle.Scale);
        }
    }
}
