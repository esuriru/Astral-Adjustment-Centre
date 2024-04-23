using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class TimelineManager : Singleton<TimelineManager>
{
    // NOTE - Make this serialized private. Not used anywhere
    public Button skipButton;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private GameObject topPanel, btmPanel;
    [SerializeField] private GameObject centerText;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private List<TimelineAsset> timelines = new List<TimelineAsset>();

    // NOTE - Make this serialized private. Not used anywhere
    public string currCutsceneName;

    // NOTE - Make this serialized private. Not used anywhere
    public string nextSceneName;
    public int cutsceneIndex = 0;

    // NOTE - Missing access specifier
    void Awake()
    {
        cutsceneIndex = 0;

        director = GetComponent<PlayableDirector>();
        director.playableAsset = timelines[cutsceneIndex];

        subtitlePanel.SetActive(false);
    }

    public IEnumerator PlayCutscene(string cutsceneName, string nextScene)
    {
        currCutsceneName = cutsceneName;
        nextSceneName = nextScene;

        switch (cutsceneName)
        {
            // NOTE - Consider using SetActive(cutsceneName = "..") so you won't
            // have one long switch
            case "PostIntro":
                skipButton.gameObject.SetActive(false);
                topPanel.SetActive(false);
                btmPanel.SetActive(false);
                centerText.SetActive(false);
                break;
            case "Lose":
                skipButton.gameObject.SetActive(true);
                topPanel.SetActive(true);
                btmPanel.SetActive(true);
                centerText.SetActive(true);
                break;
            default:
                skipButton.gameObject.SetActive(true);
                topPanel.SetActive(true);
                btmPanel.SetActive(true);
                centerText.SetActive(false);
                break;
        }

        // NOTE - Why not just cache this component?
        gameObject.GetComponent<SubtitleManager>().ResetText();

        // NOTE - This SetBGMSourcesVol should be an internal start coroutine
        // call in AudioManager
        AudioManager.Instance.StartCoroutine(AudioManager.Instance.SetBGMSourcesVol(0.2f));

        // NOTE - Use a foreach loop
        for (int i = 0; i < timelines.Count; i++)
        {
            if (timelines[i].name == (cutsceneName + "Cutscene"))
            {
                cutsceneIndex = i;
            }
        }

        director.playableAsset = timelines[cutsceneIndex];

        if (cutsceneName == "Intro")
        {
            TrackAsset targetTrack = FindTrackByName(timelines[cutsceneIndex], "CameraAnim");
            
            // NOTE - CameraHolder should be an observer to TimelineManager
            director.SetGenericBinding(targetTrack, GameObject.FindWithTag("CameraHolder").GetComponent<Animator>());
        }
        else if (cutsceneName == "Lose")
        {
            TrackAsset targetTrack = FindTrackByName(timelines[cutsceneIndex], "CameraAnim");
            
            director.SetGenericBinding(targetTrack, Camera.main.transform.gameObject.GetComponent<Animator>());
        }

        director.Play();

        yield return new WaitUntil(() => director.state == PlayState.Paused);

        yield return new WaitForSeconds(0.75f);

        if (nextScene != null)
        {
            if (cutsceneName == "Lose")
            {
                // NOTE - In my opinion, TradeButton should be subscribing to an
                // event from timeline manager instead of TimelineManager
                // straight up telling it
                StartCoroutine(GameObject.FindGameObjectWithTag("TradeButton").GetComponent<Generator3D>().ClearMap(false));
            }

            GameManager.Instance.ChangeScene(nextScene);
        }
        else
        {
            subtitlePanel.SetActive(false);
            yield return null;
        }

        // NOTE - Remove space
        yield return new WaitForSeconds (0.25f);

        subtitlePanel.SetActive(false);
    }

    public void SkipCutscene()
    {
        StopAllCoroutines();

        skipButton.gameObject.SetActive(false);

        if (director.state == PlayState.Playing)
        {
            director.Stop();
            cutsceneIndex++;
        }

        if (currCutsceneName == "Lose")
        {
            // NOTE - Look note above
            StartCoroutine(GameObject.FindGameObjectWithTag("TradeButton").GetComponent<Generator3D>().ClearMap(false));
        }

        GameManager.Instance.ChangeScene(nextSceneName);

        subtitlePanel.SetActive(false);
    }

    private TrackAsset FindTrackByName(TimelineAsset asset, string trackName)
    {
        IEnumerable<TrackAsset> outputTracks = asset.GetOutputTracks();
        // NOTE - Use track is AnimationTrack
        return outputTracks.FirstOrDefault(track => track.name == trackName && track.GetType() == typeof(AnimationTrack));
    }

    // NOTE - Missing access specifier
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            GameManager.Instance.ChangeScene("MenuScene");
        }
    }
}
