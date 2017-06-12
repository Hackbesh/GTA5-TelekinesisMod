using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace TelekinesisMod
{
    public class Telekinesis : ObservableScript
    {
        public const float MaxDistance = 100.0f;

        public const float RaycastRadius = 2.5f;

        public ReactiveProperty<Entity> TargetCandidate { get; }

        private bool hasTarget;

        public Telekinesis()
        {
            TargetCandidate =
                TickAsObservable
                .Where(_ => !hasTarget)
                .Where(_ => Game.Player.Character.Weapons.Current.Hash == WeaponHash.Unarmed)
                .Select(_ => GetTargetCandidate())
                .Where(c => c != null)
                .ResetAfter(null, TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler)
                .ToReactiveProperty()
                .AddTo(this);

            TickAsObservable
                .Where(_ => !hasTarget && Game.IsControlJustPressed(0, Control.Attack))
                .Select(_ => TargetCandidate.Value)
                .Where(t => t != null)
                .Subscribe(t => Coroutine.Start(MainCoroutine(t)))
                .AddTo(this);

            TickAsObservable
                .Select(_ => TargetCandidate.Value)
                .Where(t => t != null && !hasTarget)
                .Subscribe(t => DrawAlign(t))
                .AddTo(this);
        }

        //  ターゲット候補を取得する
        private Entity GetTargetCandidate()
        {
            var entity = World.RaycastCapsule(GameplayCamera.Position, GameplayCamera.Direction, MaxDistance, RaycastRadius, IntersectOptions.Everything, Game.Player.Character).HitEntity;

            if (entity != null && entity.IsAlive)
            {
                if (entity is Vehicle) return entity;
                if (entity is Ped ped)
                {
                    if (ped.CurrentVehicle != null)
                        return ped.CurrentVehicle;
                    return ped;
                }
            }

            return null;
        }

        //  照準を描画する
        private void DrawAlign(Entity entity)
        {
            var pos = entity.Position + new Vector3(0, 0, entity.Model.GetDimensions().Z);
            World.DrawMarker(MarkerType.CheckeredFlagCircle, pos, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.Red);
        }

        //  Pedをラグドール化する
        private void SetPedToRagdoll(Ped ped)
        {
            Function.Call(Hash.SET_PED_TO_RAGDOLL, new InputArgument[] { ped, 0, 0, 0, true, true, true });
        }

        private IEnumerable<object> MainCoroutine(Entity target)
        {
            hasTarget = true;

            if (target is Ped ped) SetPedToRagdoll(ped);
            target.SetNoCollision(Game.Player.Character, true);
            target.IsInvincible = true;
            yield return null;

            while (true)
            {
                //  プレーヤーが発砲したら抜ける
                if (Game.IsControlJustPressed(0, Control.Attack)) break;

                var dest = Game.Player.Character.Position + new Vector3(0, 0, 3);
                var dist = dest.DistanceTo(target.Position);
                var vec = (dest - target.Position).Normalized;
                var t = Math.Max(dist * 8, 1);

                //  ラグドール状態が解除されたら
                //  再びラグドール状態にする
                if (target is Ped p && !p.IsRagdoll)
                {
                    target.Velocity += new Vector3(0, 0, 0.01f);
                    SetPedToRagdoll(p);
                }

                target.Velocity = vec * t;
                yield return null;
            }

            //  方向ベクトルを求める
            var direction = GameplayCamera.Direction.Normalized;
            target.IsInvincible = false;

            //  銃の発射された方向へ飛んでいく
            for (int i = 0; i < 120; i++)
            {
                //  何かと衝突したら抜ける
                if (target.HasCollidedWithAnything) break;

                target.Velocity = direction * 100;
                yield return null;
            }

            //  爆発させて捨てる
            var distance = Game.Player.Character.Position.DistanceTo(target.Position);
            var type = distance > 15 ? ExplosionType.GasTank : ExplosionType.Bullet;
            World.AddExplosion(target.Position, type, 1.0f, 0.0f);
            hasTarget = false;
            yield break;
        }
    }
}