using Angry_Girls;

public class BattleResultChecker
{
    public bool AllEnemiesDefeated()
    {
        return GameLoader.Instance.characterManager.enemyCharacters.TrueForAll(c => c.isDead);
    }

    public bool AnyPlayersAlive()
    {
        return GameLoader.Instance.characterManager.playableCharacters.Exists(c => !c.isDead);
    }
}