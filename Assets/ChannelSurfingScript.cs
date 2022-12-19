using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class ChannelSurfingScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] buttons;
    public GameObject stat;
    public GameObject[] screens;
    public GameObject[] icons;
    public GameObject[] signals;
    public TextMesh[] displays;
    public Renderer bg;
    public Material[] bgmats;
    public Material[] icomats;
    public GameObject matstore;

    private int ch = 1;
    private int level;
    private int[] combo = new int[2];
    private bool combopause;
    private float combotime;
    private bool hl;
    private bool[] qc = new bool[2];
    private int charge;
    private float time;
    private bool restart;
    private string[] exempt = null;
    private readonly string[] colnames = new string[8] { "Red", "Orange", "Yellow", "Green", "Cyan", "Blue", "Purple", "Pink" };
    private readonly Color[] cols = new Color[8] { new Color(1, 0, 0), new Color(1, 0.5f, 0), new Color(0.8f, 0.8f, 0), new Color(0, 1, 0), new Color(0, 0.8f, 0.8f), new Color(0, 0, 1), new Color(0.5f, 0, 1), new Color(1, 0, 1) };
    private Color hiddencol;
    private int[] colselect = new int[2];
    private int prev;
    private static int modcount;
    private int freq = 1;
    private bool[] minordefects = new bool[5];
    private bool[] majordefects = new bool[5];
    private int question;
    private int[] majorcooldowns = new int[5] { -1, -1, -1, -1, -1};
    private readonly int[] cools = new int[5] { 2, 0, 8, 11, 5 };
    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        module.OnActivate = Activate;
        for (int i = 0; i < 3; i++)
            screens[i].SetActive(false);
        modcount++;
    }

    private void Activate()
    {
        stat.SetActive(false);
        if (exempt == null)
            exempt = GetComponent<KMBossModule>().GetIgnoredModules("Channel Surfing", new string[] {
                "14",
                "42",
                "501",
                "A>N<D",
                "Bamboozling Time Keeper",
                "Black Arrows",
                "Brainf---",
                "The Board Walk",
                "Busy Beaver",
                "Channel Surfing",
                "Don't Touch Anything",
                "Doomsday Button",
                "Duck Konundrum",
                "Floor Lights",
                "Forget Any Color",
                "Forget Enigma",
                "Forget Everything",
                "Forget Infinity",
                "Forget It Not",
                "Forget Maze Not",
                "Forget Me Later",
                "Forget Me Maybe",
                "Forget Me Not",
                "Forget Perspective",
                "Forget The Colors",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Iconic",
                "Keypad Directionality",
                "Kugelblitz",
                "Multitask",
                "OmegaDestroyer",
                "OmegaForget",
                "Organization",
                "Password Destroyer",
                "Purgatory",
                "RPS Judging",
                "Security Council",
                "Shoddy Chess",
                "Simon Forgets",
                "Simon's Stages",
                "Souvenir",
                "Tallordered Keys",
                "The Time Keeper",
                "Timing is Everything",
                "The Troll",
                "Turn the Key",
                "The Twin",
                "Twister",
                "Übermodule",
                "Ultimate Custom Night",
                "The Very Annoying Button",
                "Whiteout",
                "Zener Cards"
            });
        if(moduleIDCounter == moduleID)
            Audio.PlaySoundAtTransform("Dialup", transform);
        freq = modcount;
        StartCoroutine(Startup());
    }

    private IEnumerator Startup()
    {
        yield return null;
        modcount = 0;
        for(int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.81f);
            displays[6].text += ".";
        }
        yield return new WaitForSeconds(0.81f);
        if (moduleID == moduleIDCounter || restart)        
            Audio.PlaySoundAtTransform("Combo", transform);
        for (int i = 3; i < 7; i++)
            displays[i].text = "";
        screens[0].SetActive(true);
        if (!restart)
            foreach(KMSelectable button in buttons)
            {
                bool b = button == buttons[0];
                button.OnInteract = delegate () 
                {
                    if(!moduleSolved && !restart && !majordefects[3])
                    {
                        button.AddInteractionPunch(0.3f);
                        Audio.PlaySoundAtTransform("Click", button.transform);
                        StartCoroutine(Jerk(b));
                        bool c;
                        if (majordefects[4] && question > 2)
                            c = colselect[1] == prev;
                        else
                            c = (b == (colselect[0] == colselect[1])) ^ majordefects[1];
                        if (c && !majordefects[0])
                        {
                            if (ch == 399)
                                Solve();
                            else
                            {
                                ch++;
                                qc[0] = false;
                                if (ch > 5 * (level + 1) * (level + 3) && level < 6)
                                    level++;
                                displays[2].text = "CHANNEL: #" + ch;
                                if (combotime >= (TwitchPlaysActive ? 2 : 1) && combo[0] < 12)
                                    combo[0]++;
                                if (combo[0] > 0 && combo[0] % 4 == 0 && combo[1] < combo[0])
                                {
                                    combo[1] = combo[0];
                                    displays[(combo[0] / 4) + 2].text = "+";
                                    Audio.PlaySoundAtTransform("Combo", transform);
                                }
                                time += (TwitchPlaysActive ? 14 : 4) + ((combo[0] / 4) * 2f);
                                if (time >= 180f)
                                {
                                    time = 180f;
                                    if (info.GetSolvedModuleNames().Count() >= info.GetSolvableModuleNames().Count(x => !exempt.Contains(x)))
                                        Solve();
                                    else
                                        Prompt();
                                }
                                else
                                    Prompt();
                            }
                        }
                        else
                            Strike();
                    }
                    return false;
                };
                button.OnHighlight = delegate ()
                {
                    hl = true;
                    if (majordefects[3])
                    {
                        if(charge < 12)
                        {
                            Audio.PlaySoundAtTransform("Charge" + charge / 4, transform);
                            charge++;
                            if (charge % 4 == 3)
                                signals[charge / 4].SetActive(true);
                        }
                        else
                            Prompt();
                    }
                    else if (majordefects[2])
                    {
                        hiddencol = displays[0].color;
                        displays[0].color = new Color(0, 0, 0);
                    }
                };
                button.OnHighlightEnded = delegate ()
                {
                    hl = false;
                    if (majordefects[2])
                        displays[0].color = hiddencol;
                };
            }
        restart = false;
        Prompt();
        StartCoroutine("Countdown");
    }

    private IEnumerator Countdown()
    {
        if(time < 60f)
            time = 60f;
        while (!moduleSolved && time > 0)
        {
            displays[1].text = (Math.Round(time, 2)).ToString("f2");
            yield return null;
            float tick = Time.deltaTime / freq;
            time -= tick;
        }
        if (time <= 0)
            Strike();
    }

    private IEnumerator Zoom()
    {
        int[] ends = new int[2];
        if (minordefects[3])
        {
            ends[0] = 10;
            ends[1] = 80;
        }
        else
        {
            ends[0] = 500;
            ends[1] = 300;
        }
        float e = 0;
        while(e < 0.25f)
        {
            e += Time.deltaTime;
            displays[0].fontSize = (int)Mathf.Lerp(ends[0], ends[1], e * 4);
            yield return null;
        }
        displays[0].fontSize = ends[1];
    }

    private IEnumerator Jerk(bool left)
    {
        int j = left ? -10 : 10;
        float e = 0;
        while (e < 0.25f)
        {
            e += Time.deltaTime;
            screens[0].transform.localEulerAngles = new Vector3(0, Mathf.Lerp(j, 0, e * 4), 0);
            yield return null;
        }
        screens[0].transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    private IEnumerator Combo()
    {
        combotime = (TwitchPlaysActive ? 10f : 2f);
        while(combotime > 0)
        {
            if (!combopause)
                combotime -= Time.deltaTime;
            yield return null;
        }
        combo[0] = 0;
        combo[1] = 0;
        for (int i = 3; i < 6; i++)
            displays[i].text = "";
    }

    private IEnumerator Cycle()
    {
        if (question > 2)
            combopause = true;
        icons[4].GetComponent<Renderer>().material = icomats[Mathf.Clamp(question - 2, 0, 2)];
        colselect[1] = Random.Range(0, 8);
        if (majordefects[2])
        {
            hiddencol = cols[colselect[1]];
            displays[0].color = new Color(0, 0, 0);
        }
        else
            displays[0].color = cols[colselect[1]];
        while (majordefects[4])
        {
            yield return new WaitForSeconds(0.66f);
            if (question > 2 && qc[1] && Random.Range(0, 3) == 0)
            {
                qc[1] = false;
                colselect[1] = prev;
            }
            else
            {
                qc[1] = true;
                colselect[1] += Random.Range(1, 8);
                colselect[1] %= 8;
            }
            if (colselect[1] == prev && !majordefects[0])
                combopause = false;
            if (majordefects[2] && hl)
            {
                hiddencol = cols[colselect[1]];
                displays[0].color = new Color(0, 0, 0);
            }
            else
                displays[0].color = cols[colselect[1]];
        }
    }

    private IEnumerator Denied()
    {
        combopause = true;
        yield return new WaitForSeconds(2);
        majordefects[0] = false;
        Placeicons();
        combopause = false;
    }

    private void Placeicons()
    {
        float r = 0.018f * (majordefects.Count(x => x) - 1);
        for(int i = 0; i < 4; i++)
        {
            if (i == 3)
                i = 4;
            if (majordefects[i])
            {
                icons[i].SetActive(true);
                icons[i].transform.localPosition = new Vector3(r, 0.0153f, -0.0382f);
                r -= 0.036f;
            }
            else
                icons[i].SetActive(false);
        }
    }

    private IEnumerator Reveal()
    {
        char[] text = displays[0].text.ToCharArray();
        int tx = text.Length;
        char[] ntext = Enumerable.Range(0, tx).Select(x => ' ').ToArray();
        int[] order = Enumerable.Range(0, tx).ToArray().Shuffle().ToArray();
        displays[0].text = "";
        for(int i = 0; i < tx; i++)
        {
            ntext[order[i]] = text[order[i]];
            displays[0].text = new string(ntext);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Prompt()
    {
        if (majordefects[3])
        {
            combopause = false;
            screens[0].SetActive(true);
            screens[1].SetActive(false);
            bg.material = bgmats[8];
            qc[0] = true;
        }
        if (majordefects[4])
            StopCoroutine("Cycle");
        if (minordefects[4])
            StopCoroutine("Reveal");
        if(minordefects[2])
            bg.material = bgmats[8];
        if (minordefects[1])
            screens[0].transform.localScale = new Vector3(1, 1, 1);
        StopCoroutine("Combo");
        for(int i = 0; i < level - 1; i++)
        {
            minordefects[i] = Random.Range(0, (i / 2) + 2) == 0;
            majordefects[i] = false;
            majorcooldowns[i]--;
            if (majorcooldowns[i] <= 0 && Random.Range(0, 9 - level) == 0)
                majordefects[i] = true;
            if (majordefects[i])
                majorcooldowns[i] = cools[i];
        }
        if (majordefects[3])
        {
            charge = 0;
            combopause = true;
            Audio.PlaySoundAtTransform("Lost", transform);
            screens[0].SetActive(false);
            screens[1].SetActive(true);
            bg.material = bgmats[9];
            for (int i = 0; i < 3; i++)
                signals[i].SetActive(false);
        }
        else
        {
            if (majordefects[4])
            {
                question = Random.Range(0, qc[0] ? 1 : 5);
                if (question > 2)
                    prev = colselect[question - 3];
                colselect[0] = Random.Range(0, 8);
                displays[0].text = colnames[colselect[0]];
                StartCoroutine("Cycle");
            }
            else
            {
                colselect[0] = Random.Range(0, 8);
                colselect[1] = colselect[0];
                displays[0].text = colnames[colselect[0]];
                if (Random.Range(0, 2) == 1)
                {
                    colselect[1] += Random.Range(1, 8);
                    colselect[1] %= 8;
                }
                displays[0].text = colnames[colselect[0]];
                if (minordefects[2])
                    bg.material = bgmats[(colselect[1] + Random.Range(1, 8)) % 8];
            }
            if (minordefects[0])
                displays[0].text = new string(displays[0].text.ToCharArray().Shuffle());
            if (minordefects[4])
                StartCoroutine("Reveal");
            if (minordefects[1])
                screens[0].transform.localScale = new Vector3(1, 1, -1);
            StartCoroutine(Zoom());
            StartCoroutine("Combo");
            Placeicons();
            if (majordefects[0])
                StartCoroutine(Denied());
            if (majordefects[2])
            {
                hiddencol = cols[colselect[1]];
                displays[0].color = new Color(0, 0, 0);
            }
            else
                displays[0].color = cols[colselect[1]];
        }
    }

    private void Strike()
    {
        module.HandleStrike();
        if (time <= 0)
            Debug.LogFormat("[Channel Surfing #{0}] Out of time.", moduleID);
        else
        {
            Debug.LogFormat("[Channel Surfing #{0}] Incorrect response to Channel #{1}:", moduleID, ch);
            if(majordefects[0])
                Debug.LogFormat("[Channel Surfing #{0}] Responded with X icon present.", moduleID);
            else if(!majordefects[4] || question == 0)
                Debug.LogFormat("[Channel Surfing #{0}] Text did {1}match colour. {2}{3} side pressed.", moduleID, colselect[0] == colselect[1] ? "" : "not ", majordefects[1] ? "Switched sides. " : "", (colselect[0] == colselect[1]) ^ majordefects[1] ? "Right" : "Left");
            else
                Debug.LogFormat("[Channel Surfing #{0}] Responded with {1} when the previous {2} was {3}.", moduleID, colnames[colselect[1]], question == 1 ? "word" : "colour", colnames[prev]);
        }
        StopCoroutine("Countdown");
        for (int i = 0; i < 5; i++)
        {
            minordefects[i] = false;
            majordefects[i] = false;
        }
        qc[0] = true;
        combo[0] = 0;
        combo[1] = 0;
        bg.material = bgmats[8];
        for (int i = 0; i < 3; i++)
        {
            displays[i + 3].text = "";
            screens[i].SetActive(false);
        }
        displays[6].text = "CONNECTING";
        Audio.PlaySoundAtTransform("Dialup", transform);
        restart = true;
        StartCoroutine(Startup());
    }

    private void Solve()
    {
        Audio.PlaySoundAtTransform("Solve", transform);
        moduleSolved = true;
        module.HandlePass();
        foreach (GameObject s in screens)
            s.SetActive(false);
        bg.material = bgmats[9];
        stat.SetActive(true);
    }

    //twitch plays
    private int lastPressed = -1;
    private bool TwitchPlaysActive;
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press/p <left/l/right/r> (color) [Presses the left or right side of the module (optionally when the text color is 'color')] | !{0} highlight/h <left/l/right/r> [Highlights the left or right side of the module] | Highlights can be chained with spaces | If the next transmission error that occurs is an eye, then the last button pressed will not be automatically unhighlighted and must be manually dealt with using !{0} highlight/h <end/e> | On Twitch Plays some times are different, to view them use !{0} times";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (command.EqualsIgnoreCase("times"))
        {
            yield return "sendtochat Correctly responding 4 -> 14 seconds | Combo meter increase 1 -> 8 seconds | Combo meter depletes 2 -> 10 seconds";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (parameters[0].ToLowerInvariant().EqualsAny("p", "press"))
        {
            if (parameters.Length == 2)
            {
                if (parameters[1].ToLowerInvariant().EqualsAny("l", "left"))
                {
                    if (majordefects[3])
                    {
                        yield return "sendtochaterror Signal weak, please boost the signal first";
                        yield break;
                    }
                    yield return null;
                    buttons[0].OnHighlight();
                    buttons[0].OnInteract();
                    lastPressed = 0;
                    if (!majordefects[2])
                        buttons[0].OnHighlightEnded();
                }
                else if (parameters[1].ToLowerInvariant().EqualsAny("r", "right"))
                {
                    if (majordefects[3])
                    {
                        yield return "sendtochaterror Signal weak, please boost the signal first";
                        yield break;
                    }
                    yield return null;
                    buttons[1].OnHighlight();
                    buttons[1].OnInteract();
                    lastPressed = 1;
                    if (!majordefects[2])
                        buttons[1].OnHighlightEnded();
                }
            }
            else if (parameters.Length == 3)
            {
                if (parameters[1].ToLowerInvariant().EqualsAny("l", "left") || parameters[1].ToLowerInvariant().EqualsAny("r", "right"))
                {
                    for (int i = 0; i < colnames.Length; i++)
                    {
                        if (colnames[i].EqualsIgnoreCase(parameters[2]))
                        {
                            if (majordefects[3])
                            {
                                yield return "sendtochaterror Signal weak, please boost the signal first";
                                yield break;
                            }
                            if (!majordefects[4])
                            {
                                yield return "sendtochaterror There is no color cycling transmission error present";
                                yield break;
                            }
                            yield return null;
                            while (colselect[1] != i) yield return "trycancel";
                            if (parameters[1].ToLowerInvariant().EqualsAny("l", "left"))
                            {
                                buttons[0].OnHighlight();
                                buttons[0].OnInteract();
                                lastPressed = 0;
                                if (!majordefects[2])
                                    buttons[0].OnHighlightEnded();
                            }
                            else
                            {
                                buttons[1].OnHighlight();
                                buttons[1].OnInteract();
                                lastPressed = 1;
                                if (!majordefects[2])
                                    buttons[1].OnHighlightEnded();
                            }
                            break;
                        }
                    }
                }
            }
        }
        else if (parameters[0].ToLowerInvariant().EqualsAny("h", "highlight"))
        {
            if (parameters.Length == 2)
            {
                if (parameters[1].ToLowerInvariant().EqualsAny("e", "end"))
                {
                    if (!majordefects[2])
                    {
                        yield return "sendtochaterror There is no eye transmission error present";
                        yield break;
                    }
                    yield return null;
                    buttons[lastPressed].OnHighlightEnded();
                    yield break;
                }
            }
            if (parameters.Length >= 2)
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (!parameters[1].ToLowerInvariant().EqualsAny("l", "left") && !parameters[1].ToLowerInvariant().EqualsAny("r", "right"))
                        yield break;
                }
                if (majordefects[2])
                {
                    yield return "sendtochaterror Eye transmission error present, manually unhighlight first";
                    yield break;
                }
                yield return null;
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i].ToLowerInvariant().EqualsAny("l", "left"))
                    {
                        buttons[0].OnHighlight();
                        buttons[0].OnHighlightEnded();
                    }
                    else
                    {
                        buttons[1].OnHighlight();
                        buttons[1].OnHighlightEnded();
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (buttons[0].OnInteract == null || restart) yield return null;
        while (!moduleSolved)
        {
            while (majordefects[3])
            {
                int choice = Random.Range(0, 2);
                buttons[choice].OnHighlight();
                buttons[choice].OnHighlightEnded();
                yield return new WaitForSeconds(.1f);
            }
            while (majordefects[0]) yield return null;
            if (majordefects[4] && question > 2)
            {
                while (colselect[1] != prev) yield return null;
                buttons[Random.Range(0, 2)].OnInteract();
            }
            else if ((colselect[0] == colselect[1] && !majordefects[1]) || (colselect[0] != colselect[1] && majordefects[1]))
                buttons[0].OnInteract();
            else
                buttons[1].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }
}
