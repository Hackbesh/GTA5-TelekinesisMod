using System;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace TelekinesisMod
{
    public class Telekinesis : ObservableScript
    {
        public const float MaxDistance = 100.0f;

        public ReactiveProperty<Entity> TargetCandidate { get; }

        public Telekinesis()
        {
            TargetCandidate =
                TickAsObservable
                .Where(_ => Game.Player.Character.Weapons.Current.Hash == WeaponHash.Unarmed)
                .Select(_ => GetTargetCandidate())
                .Where(c => c != null)
                .ResetAfter(null, TimeSpan.FromSeconds(2))
                .ObserveOn(Scheduler)
                .ToReactiveProperty()
                .AddTo(this);

            TargetCandidate.Subscribe(t => UI.ShowSubtitle($"{(t == null ? "null" : "non-null")}")).AddTo(this);
        }

        //  ターゲット候補を取得する
        private Entity GetTargetCandidate()
        {
            var entity = World.RaycastCapsule(GameplayCamera.Position, GameplayCamera.Direction, MaxDistance, 2.0f, IntersectOptions.Everything, Game.Player.Character).HitEntity;
            return entity is Ped || entity is Vehicle ? entity : null;
        }
    }
}