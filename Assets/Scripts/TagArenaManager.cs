using UnityEngine;
using Unity.MLAgents;

public class TagArenaManager : MonoBehaviour
{
    public TagAgent chaser;
    public TagAgent runner;
    public float arenaRadius = 4f;

    public void ResetArena()
    {
        chaser.transform.localPosition = new Vector3(
            Random.Range(-arenaRadius, -1f),
            1f,
            Random.Range(-arenaRadius, arenaRadius)
        );
        runner.transform.localPosition = new Vector3(
            Random.Range(1f, arenaRadius),
            1f,
            Random.Range(-arenaRadius, arenaRadius)
        );

        chaser.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        runner.transform.rotation  = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        chaser.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        chaser.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        runner.GetComponent<Rigidbody>().linearVelocity  = Vector3.zero;
        runner.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public TagAgent GetOpponent(TagAgent agent)
    {
        return agent == chaser ? runner : chaser;
    }

    public void OnAgentTagged(TagAgent tagger, TagAgent tagged)
    {
        // Safe max step values — never allow division by zero
        int taggerMax = (tagger.MaxStep > 0) ? tagger.MaxStep : 5000;
        int taggedMax = (tagged.MaxStep > 0) ? tagged.MaxStep : 5000;

        // Clamp StepCount so we never exceed MaxStep (avoids > 1 ratios)
        float taggerSteps = Mathf.Clamp(tagger.StepCount, 0, taggerMax);
        float taggedSteps = Mathf.Clamp(tagged.StepCount, 0, taggedMax);

        if (tagger.teamId == 0) // chaser tagged the runner
        {
            // Time bonus: faster catch = higher reward (range: 0 to 0.5)
            float timeBonus = Mathf.Clamp01(1f - (taggerSteps / taggerMax)) * 0.5f;
            tagger.AddReward(1f + timeBonus);

            // Survival bonus for runner: longer survival = smaller penalty
            float survivalBonus = Mathf.Clamp01(taggedSteps / taggedMax) * 0.5f;
            tagged.AddReward(-1f + survivalBonus);
        }
        else
        {
            // Fallback symmetric reward for edge cases
            tagger.AddReward(1f);
            tagged.AddReward(-1f);
        }

        tagger.EndEpisode();
        tagged.EndEpisode();
    }
}