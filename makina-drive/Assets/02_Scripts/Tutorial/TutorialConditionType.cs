
public enum TutorialConditionType 
{ 
    MoveToArea,         // 特定のポイントに移動する
    PerformAction,      // 特定のアクションを行う
    AttackEnemy,        // 特定の攻撃を当てる
    DefeatEnemy,        // 敵を撃破する
    HitAttackTag,       // 特定の種類（タグ）の攻撃を当てる
    DamageUnitWithTag,  // 特定のタグのユニットにダメージを与える
    
    DamageSpecificUnit, // 特定の名前のユニットにダメージを与える
}