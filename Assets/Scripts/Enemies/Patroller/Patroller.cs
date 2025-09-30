using UnityEngine;

public class Patroller : EnemyBase
{
    [SerializeField] Transform leftEdge, rightEdge;
    [SerializeField] float speed = 2f;
    StateMachine fsm;

    class WalkState : IState
    {
        Patroller p; int dir; // -1 left, +1 right
        public WalkState(Patroller p, int dir) { this.p = p; this.dir = dir; }
        public void OnEnter() {}
        public void OnExit() {}
        public void Tick() {
            p.rb.linearVelocity = new Vector2(dir * p.speed, p.rb.linearVelocity.y);
            if (dir < 0 && p.transform.position.x <= p.leftEdge.position.x) p.fsm.Set(new WalkState(p, +1));
            if (dir > 0 && p.transform.position.x >= p.rightEdge.position.x) p.fsm.Set(new WalkState(p, -1));
        }
    }

    protected override void Start() {
        base.Start();
        fsm = new StateMachine();
        fsm.Set(new WalkState(this, +1));
    }

    void Update() => fsm.Tick();
}

