using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public GameObject map;
    [SerializeField] private GameObject fullMap;
    [SerializeField] private GameObject mapYamaRoot;
    [SerializeField] private Animator mapFinishAnim;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Interactable firstChessInteractable;
    [SerializeField] private GameObject whale;
    [SerializeField] private GameObject whaleTurnPov;
    [SerializeField] private CapsuleCollider blockCol;
    [SerializeField] private Animator blackout;

    public GameObject puzzle1;
    public GameObject puzzle2;
    public GameObject puzzle3;
    public GameObject puzzle4;
    public GameObject puzzle5;
    public GameObject puzzle6;

    public GameObject balina3D;
    public GameObject balina2D;
    public GameObject goblinPrinces;
    public GameObject mainMap;

    public GameObject hayalet;
    public GameObject prens;

    public GameObject[] bodyParts = new GameObject[9];
    public Transform holdObjectTransform;
    public GameObject[] settledBodyParts = new GameObject[9];
    public GameObject[] boddiesToCollect = new GameObject[9];   
    private bool bodiesCollectable = false;

    public int bodyCount = 0;
    private bool firstWhaleInteractionDone = false;
    private bool firstSpokenGhost = false;
    private bool firstSpokenWhale = false;
    public bool[] mapReceived = {false, false, false, false, false, false};
    public bool mapAssured = false;
    public bool firstChestOpenable = false;

    public bool mapFinishEventTrigger = false;
    private bool lastWhaleInteraction = false;

    public Transform[] wakeTransforms = new Transform[5];
    public GameObject actionCam;
    public GameObject defaultCam;
    public GameObject DizzyFilter;
    public GameObject princessHead;
    KeyValuePair<int, GameObject> holdedBodyPart;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        StartCoroutine(startSceneSequance());
    }
    public void FixedUpdate()
    {
        if (mapFinishEventTrigger)
        {
            mapFinishEventTrigger = false;
            Invoke("mapFinishEvent", 1.8f);
        }
    }

    private IEnumerator startSceneSequance()
    {
        PlayerMovementManager.Instance.StopMovement();

        DizzyFilter.SetActive(true);
        if (defaultCam != null) defaultCam.SetActive(false);
        if (actionCam != null) actionCam.SetActive(true);

        Canvas mainCanvas = DizzyFilter.GetComponentInParent<Canvas>();
        if (mainCanvas != null && actionCam != null)
        {
            mainCanvas.worldCamera = actionCam.GetComponent<Camera>();
        }

        foreach (Transform wakeTransform in wakeTransforms)
        {
            yield return new WaitForSeconds(2f);

            actionCam.transform.position = wakeTransform.position;
            actionCam.transform.rotation = wakeTransform.rotation;

            blackout.SetTrigger("light");
            DizzyFilter.SetActive(true);
            StartCoroutine(DizzyCameraShake(actionCam.transform, wakeTransform.position, 3f, 0.15f));

            yield return new WaitForSeconds(3f);
            blackout.SetTrigger("blackout");
            DizzyFilter.SetActive(false);
        }

        yield return new WaitForSeconds(2f);
        blackout.SetTrigger("light");
        DizzyFilter.GetComponent<Animator>().SetTrigger("closeDizzy");

        if (actionCam != null) actionCam.SetActive(false);
        if (defaultCam != null) defaultCam.SetActive(true);

        if (mainCanvas != null && defaultCam != null)
        {
            mainCanvas.worldCamera = defaultCam.GetComponent<Camera>();
        }

        List<DialogueLine> conversation1 = new List<DialogueLine>
    {
        new DialogueLine("Prenses", "Ayyy noluyor be her yanim kramp... Kafama geçirdiler sanki"),
        new DialogueLine("Prenses", "Her yanım tutuldu başım davul gibi")
    };

        DialogueManager.Instance.StartDialogue(conversation1, () =>
        {
            DizzyFilter.SetActive(false);
            CameraStateController.Instance.IncreaseFOVTo(30);
            CameraStateController.Instance.StartOrbitSequence(princessHead, 3f, 60, 10);

            List<DialogueLine> conversation2 = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Neresi burası be şatom nerede benim ayol???"),
            new DialogueLine("Prenses", "PREEEEEEENNSSSS!!!"),
            new DialogueLine("Prenses", "ASKERLEEEEEEERRRRR!!!"),
            new DialogueLine("Prenses", "Nerdesin be adam???"),
            new DialogueLine("Prenses", "Etrafa bakınsam iyi olacak...")
        };

            DialogueManager.Instance.StartDialogue(conversation2, () =>
            {
                PlayerMovementManager.Instance.ResumeMovement();
                CameraStateController.Instance.ResetFOV();
            });
        });
    }

    private IEnumerator DizzyCameraShake(Transform camTransform, Vector3 basePosition, float duration, float magnitude)
    {
        float elapsed = 0.0f;
        float randomStart = Random.Range(0f, 100f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = (Mathf.PerlinNoise(randomStart + elapsed * 2f, 0f) * 2f - 1f) * magnitude;
            float y = (Mathf.PerlinNoise(0f, randomStart + elapsed * 2f) * 2f - 1f) * magnitude;

            camTransform.position = basePosition + camTransform.right * x + camTransform.up * y;

            yield return null;
        }

        camTransform.position = basePosition;
    }
    private void mapFinishEvent()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Whale", "Haritaları tamamladın, iyi iş çıkardın fıstık."),
            new DialogueLine("Whale", "Bu noktada sana bir şey itiraf etmek istiyorum fıstık..."),
            new DialogueLine("Prenses", "Öncelikle bidaha goblin prensesine fıstık demeye cürret edersen insanlardan önce ben zıpkınlatırım seni... Heh et şimdi ne itirafı?"),
            new DialogueLine("Whale", "Ee şey... İşin aslı şu ki."),
            new DialogueLine("Whale", "Insanların beni avlayıp yağımı çıkartmaya çalıştığı falan yok, orta çağda yaşamıyoruz ben korunan bir türüm."),
            new DialogueLine("Prenses", "!!!!????"),
            new DialogueLine("Whale", "Sadece birilerini ağzıma almak hoşuma gidiyor... Bu yüzden sana yalan söyledim. Özür dilerim..."),
            new DialogueLine("Prenses", "ÖĞĞĞKK..."),
            new DialogueLine("Prenses", "Son adaya gitmek için lazım olmasan var ya SENI PARAMPINCIK EDERDIM BURDA TMAM MIĞĞ????"),
            new DialogueLine("Whale", "Tamam sakin ol fıstık, ha bak sana ufak bir özür yardimim dokunabilir."),
            new DialogueLine("Whale", "Sandıkların yerlerini bulurken adada rastgele koşup E spamlamak yerine haritana bakarak bulduysan farketmişsindir..."),
            new DialogueLine("Whale", "Haritanda adalar dışındaki yerler hala eksik... "),
            new DialogueLine("Whale", "28 yıllık balina tecrübemle eksik kısımları avucumun içi gibi biliyorum."),
            new DialogueLine("Whale", "NAME NAME ADANEH ALO NOLA... PISTACHIO MAPOO HOOOOOOOOMA !!!"),
            new DialogueLine("Prenses", "Noluyor NAPIYORSUN BE????"),
            new DialogueLine("Prenses", "AAAAIIYYY !!!????"),
            new DialogueLine("Prenses", "Haritam titreşiyor AYOOĞĞL...")
        };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameManager.Instance.ChangeState(GameState.Map);
            InputManager.Instance.StopInteraction();
            openMap();
            mapFinishAnim.SetTrigger("mapdone");
            Invoke("invokedFullMap", 1f);
            PlayerMovementManager.Instance.ResumeMovement();
        });
    }
    private void invokedFullMap()
    {
        fullMap.SetActive(true);
        mapYamaRoot.SetActive(false);
        InputManager.Instance.ResumeInteraction();
        lastWhaleInteraction = true;
    }
    public void ToggleMap()
    {
        if (GameManager.Instance.CurrentState == GameState.Exploring)
        {
            GameManager.Instance.ChangeState(GameState.Map);
            openMap();
        }
        else if (GameManager.Instance.CurrentState == GameState.Map)
        {
            GameManager.Instance.ChangeState(GameState.Exploring);
            closeMap();
        }
    }

    public void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.Exploring)
        {
            GameManager.Instance.ChangeState(GameState.Paused);
            pause();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.ChangeState(GameState.Exploring);
            resume();
        }
    }

    public void openMap()
    {
        map.SetActive(true);
    }
    public void closeMap()
    {
        map.SetActive(false);   
    }
    public void pause()
    {
        pauseScreen.SetActive(true);
        Time.timeScale = 0f; 
    }
    public void resume()
    {
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
    }

    public void whaleOn1()
    {
        if (!firstWhaleInteractionDone)
        {
            if(!firstSpokenGhost)
            {
                if (!firstSpokenWhale)
                {
                    List<DialogueLine> conversation1 = new List<DialogueLine>
                    {
                        new DialogueLine("Whale", "Naber fıstık, Balina Oti'nin ağzına girmek ister misin :)"),
                        new DialogueLine("Prenses", "Aiiiiyyyy, üstüme iyilik saglik ayollll, pis ucube balik..."),
                        new DialogueLine("Whale", "Ama ben... kocan prens benden sana..."),
                        new DialogueLine("Prenses", "Ayyy, defoooğğğlll"),
                        new DialogueLine("Whale", "Ama... goblin prens sana yardım etmemi..."),
                        new DialogueLine("Prenses", "USTUME IYILIK SAĞLIĞĞĞĞKKK...KIM OLDUGUNU SANIYORSUN SEN??? EMBESIL UCUBE ASIMETRIK OBEZ BALIK SENI."),
                        new DialogueLine("Prenses", "Çekici guclu bir kadin gordun ve yararlanmak istedin ha."),
                        new DialogueLine("Prenses", "Hepiniz aynisiniz tum erkolar, Allahinizdan bulun..."),
                        new DialogueLine("Whale", "...")
                    };

                    DialogueManager.Instance.StartDialogue(conversation1, () =>
                    {
                        List<DialogueLine> conversation2 = new List<DialogueLine>
                    {
                        new DialogueLine("Hayalet", "PIŞŞŞTTT"),
                        new DialogueLine("Hayalet", "Radikal feminist goblin prensesiii!!!"),
                        new DialogueLine("Hayalet", "Buradayım pşştt soluna bak..."),
                    };

                        DialogueManager.Instance.StartDialogue(conversation2, () =>
                        {
                            hayalet.SetActive(true);
                            firstSpokenWhale = true;
                        });
                    });
                }
                else
                {
                    List<DialogueLine> conversation3 = new List<DialogueLine>
                    {
                        new DialogueLine("Whale", "Defol git be seninle mi uğraşıcam deli karı, bok girersin ağzıma şu saatten sonra..."),
                        new DialogueLine("Prenses", "Bak hala girersin diyor siktir ordan sapik herif..."),
                    };
                    DialogueManager.Instance.StartDialogue(conversation3);
                }
            }
            else
            {
                if(mapReceived[1])
                {
                    List<DialogueLine> conversation4 = new List<DialogueLine>
                    {
                        new DialogueLine("Whale", "Dua et ki goblin prensi senin adına özür diledi. Ve ona borcluyum sırf onun için sana kat..."),
                        new DialogueLine("Prenses", "Aman sus be anladık. Diğer adanın haritası da hazır, eğil de bineyim hadi."),
                        new DialogueLine("Whale", "Bineyim mi? Nefes al diye yüzeyden gidelim de mızraklayıp yağımı mı çıkartsinlar???"),
                        new DialogueLine("Whale", "Ağzıma gireceksin..."),
                        new DialogueLine("Prenses", "Girmeyeceğim..."),
                        new DialogueLine("Whale", "Gireceksin..."),
                        new DialogueLine("Prenses", "...Hıh"),
                        new DialogueLine("Prenses", "Kocacuğum için.")
                    };
                    DialogueManager.Instance.StartDialogue(conversation4, () =>
                    {
                        WhaleManager.Instance.EnterWhaleMode();
                        firstWhaleInteractionDone = true;
                        mapAssured = false;
                    });
                }
                else
                {
                    List<DialogueLine> conversation5 = new List<DialogueLine>
                    {
                        new DialogueLine("Whale", "Noldu lan şirret kadin..."),
                        new DialogueLine("Whale", "Muhtac mi kaldin ASIMETRIK OBEZ UCUBE BALIGINA??!"),
                        new DialogueLine("Whale", "Defol git babaya binersin daha gideceğin adanın haritası bile yok elinde."),
                        new DialogueLine("Prenses", "Artık şu haritayı bulsam iyi olacak..."),
                    };
                    DialogueManager.Instance.StartDialogue(conversation5, () =>
                    {
                        firstChestOpenable = true;
                        firstChessInteractable.destroyAfterInteraction = true;
                        firstChessInteractable.animateOnInteract = true;
                        firstChessInteractable.invokeDelay = 2f;
                    });
                }
            }
            return;
        }
        else
        {
            if (mapAssured)
            {
                if (lastWhaleInteraction)
                {
                    lastWhaleInteraction = false;
                    List<DialogueLine> conversation7 = new List<DialogueLine>
                    {
                        new DialogueLine("Prenses", "Eğil biniym götür şuraya çabuk pis sapik..."),
                        new DialogueLine("Whale", ":("),
                        new DialogueLine("Whale", "Oti'ye son bir kez katlansan olmaz mı okadar geçmişimiz oldu... :("),
                        new DialogueLine("Prenses", "Öf aman be götümüzün dibinde 5. ada zaten aç hadi.")
                    };

                    DialogueManager.Instance.StartDialogue(conversation7, () =>
                    {
                        WhaleManager.Instance.EnterWhaleMode();
                    });
                }
                else
                {
                    List<DialogueLine> conversation7 = new List<DialogueLine>
                    {
                        new DialogueLine("Whale", "Gir bakalm fıstık?")
                    };

                    DialogueManager.Instance.StartDialogue(conversation7, () =>
                    {
                        WhaleManager.Instance.EnterWhaleMode();
                    });
                }
            }
            else 
            {                 
                List<DialogueLine> conversation8 = new List<DialogueLine>
                {
                    new DialogueLine("Whale", "Ağız kokumu mu özledin?"),
                    new DialogueLine("Whale", "Gideceğimiz yerin haritasını çözmeden nereye gitmeyi planlıyorsun?"),
                    new DialogueLine("Prenses", "Hmm doru. Tamam uzatma erko.")
                };
                DialogueManager.Instance.StartDialogue(conversation8);
            }
        }
    }

    public void restrictedKraken()
    {
        WhaleManager.Instance.StopWhaleMovement();
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Whale", "Krakene tükürürürüm seni, zaten tiltim... Yolunu değiştir."),
            new DialogueLine("Prenses", "...")
        };

        DialogueManager.Instance.StartDialogue(conversation,() => 
        {
            WhaleManager.Instance.ResumeWhaleMovement();
        });
        GameManager.Instance.ChangeState(GameState.OnWhale);
    }

    public void startIsland2()
    {
        mapAssured = false;
        balina3D.transform.position = new Vector3(3.802118f, -3.538488f, -8.858302f);
        balina3D.transform.rotation = Quaternion.Euler(28.979f, 2.926f, -6.507f);
        goblinPrinces.transform.position = new Vector3(7.37f, 1.78f, -14.69f);
        Physics.SyncTransforms();
        balina2D.GetComponent<RectTransform>().anchoredPosition = new Vector2(69f, -155f);
        WhaleManager.Instance.ExitWhaleMode();
    }
    public void startIsland3()
    {
        mapAssured = false;
        balina3D.transform.position = new Vector3(27.58f, -3.538488f, 10.52f);
        balina3D.transform.rotation = Quaternion.Euler(28.979f, 2.926f, -6.507f);
        goblinPrinces.transform.position = new Vector3(27.1f, 2.96f, 2.2f);
        Physics.SyncTransforms();
        balina2D.GetComponent<RectTransform>().anchoredPosition = new Vector2(329f, 85f);
        WhaleManager.Instance.ExitWhaleMode();
    }
    public void startIsland4()
    {
        mapAssured = false;
        balina3D.transform.position = new Vector3(3.598696f, -3.538488f, 25.09f);
        balina3D.transform.rotation = Quaternion.Euler(28.979f, -37.368f, -6.507f);
        goblinPrinces.transform.position = new Vector3(5.62f, 2.04f, 19.54f);
        Physics.SyncTransforms();
        balina2D.GetComponent<RectTransform>().anchoredPosition = new Vector2(243f, 293f);
        WhaleManager.Instance.ExitWhaleMode();
    }
    public void startIsland5()
    {
        WhaleManager.Instance.StopWhaleMovement();
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Whale", "Inmeden once..."),
            new DialogueLine("Whale", "Her şey için özür dilemek istedim..."),
            new DialogueLine("Whale", "Sana yalan söyledim, seni kandırdım, şimdi de..."),
            new DialogueLine("Whale", "Inmene izin vermeyeceğim."),
            new DialogueLine("Prenses", "NE??!!"),
            new DialogueLine("Whale", "Seni şuan indirirsem birdaha ağzıma girmeyeceksin..."),
            new DialogueLine("Prenses", "AAAAAAAAAAAAAAiiiiiiĞĞYYY!?!! Sal lan beni ucube balık!!!"),
            new DialogueLine("Prenses", "Yemin ederim boğazından içeriye sıçarım aha şuradan. Literal yaparim..."),
            new DialogueLine("Prenses", "Ahanda tam şuraya..."),
            new DialogueLine("Whale", "Tamam dur sakin ol anlaşabili...RĞĞKKKK")
        };

        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            WhaleManager.Instance.FluctuateWhale(1f, 20f);
            List<DialogueLine> conversation1 = new List<DialogueLine>
            {
                new DialogueLine("Whale", "ÖÖÖÖÖĞĞĞYYYKKKKKK"),
                new DialogueLine("Whale", "FOOOŞŞŞŞURRRTTTT (Kusma sesleri)")
            };
            DialogueManager.Instance.StartDialogue(conversation1, () =>
            {
                WhaleManager.Instance.ResumeWhaleMovement();
                goblinPrinces.transform.position = new Vector3(-7.11f, 6.3f, 22.53f);
                Physics.SyncTransforms();
                WhaleManager.Instance.ExitWhaleMode();
                GameManager.Instance.ChangeState(GameState.Exploring);
                map.SetActive(false);

                List<DialogueLine> conversation2 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", "Ne yaşadim ben ayol..."),
                    new DialogueLine("Prenses", "Bulacagin tekneye sokim kocacigim."),
                    new DialogueLine("Prenses", "Insanların prensi yat yat gezdirir, bizim goblin prensi balina ağzında ceset toplatıyor...")
                };
                DialogueManager.Instance.StartDialogue(conversation2);
            });
        });
    }

    public void interactFirstMap()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Ne istiyorsun be hödük, basima gelmeyen kalmadi zaten..."),
            new DialogueLine("Hayalet", "Prensesimmm..."),
            new DialogueLine("Hayalet", "Ben sevgili kocacığın goblin prens."),
            new DialogueLine("Hayalet", "Vücudumu kaybettim. Yardımına ihtiyacim var direk konuya giriyorum."),
            new DialogueLine("Hayalet", "İnsanlar beni parçalara ayırdı."),
            new DialogueLine("Hayalet", "Bizden asırlardır korkuyorlar su boyunda balina beslerken tek düşürdüler."),
            new DialogueLine("Hayalet", "Neden mi sana dokunmadılar? Radikal feminizminin onları kaçırdığından süpheleniyorum."),
            new DialogueLine("Hayalet", "Şey... Insan ırkı bu konuda biraz hassas sanirim..."),
            new DialogueLine("Hayalet", "Her neyse, beni diriltmek için birtek sana güvenebilirim. Bedenimin parçalarını beni kaçırdıkları adaya dağıttılar."),
            new DialogueLine("Hayalet", "Tüm parcalarimi bulup birleştirmelisin, beni diriltmenin tek yolu bu."),
            new DialogueLine("Hayalet", "Yol üzerinde gideceğin bir sonraki ada için ipuçları içeren harita parçaları bulacaksın."),
            new DialogueLine("Hayalet", "Al sana bu adanın harita parçaları..."),
            new DialogueLine("Hayalet", "Sonrakileri kendin bulman gerekecek."),
            new DialogueLine("Prenses", "OFF. Ama çok işş"),
            new DialogueLine("Hayalet", "Sen bu işi slaylersin."),
            new DialogueLine("Hayalet", "Hadi ben kaçtım zebani hanım bekliyo..."),
            new DialogueLine("Hayalet", "Öhö.. aman dürtüyo...")
        };

        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            puzzle1.SetActive(true);
            hayalet.GetComponent<BoxCollider>().enabled = false;
            hayalet.SetActive(false);
            firstSpokenGhost = true;
        });
    }
    public void interactSecondMap()
    {
        if(!firstChestOpenable)
        {
            List<DialogueLine> conversation1 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Hmm napim ben bu sandığı?"),
                new DialogueLine("Prenses", "Önce adada ne olup bittiğini çözmeliyim..."),
            };
            DialogueManager.Instance.StartDialogue(conversation1);
            return;
        }
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Ikinci harita kalıntıları da buldum aşkoma adim adim..."),
            new DialogueLine("Hayalet", "Aferin prenses. Senin için sahil kenarındaki eski dostumla anlaştim... Balina Oti."),
            new DialogueLine("Hayalet", "Adalar arasi seyahat etmen icin sana yardim edecek."),
            new DialogueLine("Hayalet", "Haritana bakarak Oti'nin yerini bulabilirsin...")
        };

        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            PlayerMovementManager.Instance.ResumeMovement();
            puzzle2.SetActive(true);
        });
    }

    public void interactThirdMap()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Üçüncü harita kalıntılarını da buldum..."),
            new DialogueLine("Hayalet", "Çok az kaldı son adaya vardığında benim parçalarımı birleştirebileceğin bir çember bulacaksın.")
        };

        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            PlayerMovementManager.Instance.ResumeMovement();
            puzzle3.SetActive(true);
        });
    }

    public void interactFourthMap()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Dördüncü harita kalıntılarını da buldum...")
        };

        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            PlayerMovementManager.Instance.ResumeMovement();
            puzzle4.SetActive(true);
        });
    }

    public void interactFifthMap()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Bu da son harita olmalı."),
            new DialogueLine("Prenses", "Bebeğimin parçalarını toplamak için son adaya son bir iğrenc yolculuk."),
            new DialogueLine("Hayalet", "Az kaldı seni bekliyorum canım.")
        };

        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            PlayerMovementManager.Instance.ResumeMovement();
            puzzle5.SetActive(true);
        });
    }

    public void interactCatacoumb()
    {
        if (bodyCount < 9)
        {
            if (holdedBodyPart.Value != null)
            {
                bodyCount++;
                Destroy(holdedBodyPart.Value);
                settledBodyParts[holdedBodyPart.Key].SetActive(true);
                holdedBodyPart = new KeyValuePair<int, GameObject>(-1, null);
                if (bodyCount == 9)
                {
                    List<DialogueLine> conversation3 = new List<DialogueLine>
                    {
                        new DialogueLine("Prenses", "Inşallah tüm parçalarini toplamişimdir."),
                        new DialogueLine("Prenses", "Artık eksikse de biyeri eksik kalıversin yoruldum yeter."),
                        new DialogueLine("Hayalet", "Aman iyice bak oraya buraya kalmasin eksik bişeyim..."),
                        new DialogueLine("Prenses", "Yaw sanki her şeyin çok tam da konuşturma şimdi beni..."),
                        new DialogueLine("Hayalet", "..."),
                        new DialogueLine("Prenses", "Tamam tamam, bakıyorum merak etme")
                    };
                    DialogueManager.Instance.StartDialogue(conversation3);
                    return;
                } 
                List<DialogueLine> conversation2 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", "Oh, kaldı geriye " + (9 - bodyCount) + " parça.")
                };
                DialogueManager.Instance.StartDialogue(conversation2);
                return;
            }
            else
            {
                if (bodyCount == 0)
                {
                    List<DialogueLine> conversation1 = new List<DialogueLine>
                    {
                        new DialogueLine("Prenses", "Sanırım canımın icini burada yapistiricam."),
                        new DialogueLine("Prenses", "Haritami kullanip cesedinin parcalarini bulmaliyim.")
                    };
                    DialogueManager.Instance.StartDialogue(conversation1, () =>
                    {
                        bodiesCollectable = true;
                    });
                    return;
                }
                string message = "9 parça içinden " + bodyCount + " parça buldum.";
                List<DialogueLine> conversation2 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", message),
                    new DialogueLine("Prenses", "Bekle beni aşkitom.")
                };
                DialogueManager.Instance.StartDialogue(conversation2);
                return;
            }
        }
        
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Uzun bekleyişin sonu geldi. Umarım yeterince hızlı olurum."),
            new DialogueLine("Prenses", "Bekle beni bebeğim.")
        };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            PlayerMovementManager.Instance.ResumeMovement();
            puzzle6.SetActive(true);
        });
        
    }
    public void initiateFinalPuzzleFinish(GameObject puzzleObj)
    {
        StartCoroutine(initiateFinalPuzzleFinishCoroutine(puzzleObj));
    }

    public IEnumerator initiateFinalPuzzleFinishCoroutine(GameObject puzzleObj)
    {
        PlayerMovementManager.Instance.StopMovement();
        CameraStateController.Instance.IncreaseFOVTo(60f);
        yield return new WaitForSeconds(0.8f);

        hayalet.SetActive(true);
        hayalet.GetComponent<Animator>().SetTrigger("otherIdle");

        hayalet.transform.position = new Vector3(-10.86f, 5.049f, 10.281f);

        Vector3 yon = goblinPrinces.transform.position - hayalet.transform.position;
        float yAcisi = Mathf.Atan2(yon.x, yon.z) * Mathf.Rad2Deg;
        hayalet.transform.rotation = Quaternion.Euler(0f, -yAcisi, 0f);

        puzzleObj.SetActive(false);
        prens.SetActive(true);

        for (int i = 0; i < 9; i++)
        {
            settledBodyParts[i].SetActive(false);
        }

        List<DialogueLine> conversation7 = new List<DialogueLine>
        {
            new DialogueLine("Hayalet", "Uyanma vaktim geldi. Göster marifetlerini güzelim sok beni fani haşmetli bedenime...."),
            new DialogueLine("Prenses", "OOOOO MATATA HOLO..."),
            new DialogueLine("Prenses", "OOOO MATATA YOLA.. KAMAHA"),
            new DialogueLine("Prenses", "NAAAMANA RAMANE HOOO MATATA ANNEANANE ANANA"),
            new DialogueLine("Prenses", "Uyansana yoruldum ayol..."),
            new DialogueLine("Prenses", "AMONE PORTE KORANANA MAKATTA YAMANNA"),
            new DialogueLine("Hayalet", "GIRIYORUMMM BEDENIMI HISSEDEBILIYORUM BAGLANMAK UZEREYIM AAGGGGGHHHH."),
            new DialogueLine("Whale", "Nereye giriosan gir be adam kafamiza ettin"),
            new DialogueLine("Prens", "AGGHAHAAGGGG")
        };

        DialogueManager.Instance.StartDialogue(conversation7, () =>
        {
            StartCoroutine(AfterFirstDialogue());
        });

        IEnumerator AfterFirstDialogue()
        {
            goblinPrinces.GetComponent<Animator>().SetTrigger("IsHappy");

            List<DialogueLine> conversation8 = new List<DialogueLine>
            {
                new DialogueLine("Prens", "HISSEDEBILIYORUM...")
            };

            DialogueManager.Instance.StartDialogue(conversation8, () =>
            {
                StartCoroutine(AfterSecondDialogue());
            });
            yield break;
        }

        IEnumerator AfterSecondDialogue()
        {
            hayalet.GetComponent<Animator>().SetTrigger("dead");
            prens.GetComponent<Animator>().SetTrigger("diril");
            yield return new WaitForSeconds(4f);
            StartCoroutine(twerk());
        }
    }

    private IEnumerator twerk()
    {
        Vector3 yon = goblinPrinces.transform.position - prens.transform.position;
        float yAcisi = Mathf.Atan2(yon.x, yon.z) * Mathf.Rad2Deg;
        prens.transform.rotation = Quaternion.Euler(0f, -yAcisi, 0f);

        List<DialogueLine> conversation7 = new List<DialogueLine>
        {
            new DialogueLine("Prens", "BASARDIN"),
            new DialogueLine("Prens", "HISSEDEBILIYORUM..."),
            new DialogueLine("Prens", "Kollarımmm, bacaklarımm"),
            new DialogueLine("Prens", "..."),
            new DialogueLine("Prens", "POPOMM")
        };

        DialogueManager.Instance.StartDialogue(conversation7, () =>
        {
            StartCoroutine(AfterTwerkDialogue());
        });

        yield break;

        IEnumerator AfterTwerkDialogue()
        {
            CameraStateController.Instance.ResetFOV();
            yield return new WaitForSeconds(1f);

            prens.GetComponent<Animator>().SetBool("twerking", true);

            yield return new WaitForSeconds(1f);
            CameraStateController.Instance.StartOrbitSequence(prens, 5, 80, 20f);
            yield return new WaitForSeconds(5f);

            goblinPrinces.GetComponent<Animator>().SetTrigger("IsSad");

            yield return new WaitForSeconds(3.5f);

            CameraStateController.Instance.IncreaseFOVTo(60f);   

            List<DialogueLine> finalConversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ayyy"),
                new DialogueLine("Prenses", "Oldu kizz.. Oynuo orasi burasi valla"),
                new DialogueLine("Prens", "Vakit intikam vaktidir feminist goblin prensesi..."),
                new DialogueLine("Prens", "Insanlari diyarın sularina gömüp goblinleri eski ihtisamina kavusturacagiz...")
            };

            DialogueManager.Instance.StartDialogue(finalConversation, () =>
            {
                StartCoroutine(FinalBlackoutSequence());
            });
        }

        IEnumerator FinalBlackoutSequence()
        {
            CameraStateController.Instance.StartOrbitSequence(prens, 5, 60, 30f);
            yield return new WaitForSeconds(5f);
            CameraStateController.Instance.IncreaseFOVTo(90f);
            balina3D.transform.position = new Vector3(-4.05f, -1.89f, -2.17f);
            //balina3D.transform.rotation = Quaternion.Euler(28.979f, 2.926f, -6.507f);
            CameraStateController.Instance.StartOrbitSequence(whaleTurnPov, 10, 70, 18f);
            yield return new WaitForSeconds(9f);
            blackout.SetTrigger("blackout");
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    public void addBodyCount()
    {
        bodyCount++;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation);
    }
    public void takeBodyPart1()
    {
        if(!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[0], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(0, holding);
            boddiesToCollect[0].SetActive(false);
        });
    }
    public void takeBodyPart2()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[1], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(1, holding);
            boddiesToCollect[1].SetActive(false);
        });
    }
    public void takeBodyPart3()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[2], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(2, holding);
            boddiesToCollect[2].SetActive(false);
        });
    }
    public void takeBodyPart4()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[3], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(3, holding);
            boddiesToCollect[3].SetActive(false);
        });
    }
    public void takeBodyPart5()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım.")
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[4], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(4, holding);
            boddiesToCollect[4].SetActive(false);
        });
    }
    public void takeBodyPart6()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[5], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(5, holding);
            boddiesToCollect[5].SetActive(false);
        });
    }
    public void takeBodyPart7()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[6], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(6, holding);
            boddiesToCollect[6].SetActive(false);
        });
    }
    public void takeBodyPart8()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[7], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(7, holding);
            boddiesToCollect[7].SetActive(false);
        });
    }
    public void takeBodyPart9()
    {
        if (!bodiesCollectable)
        {
            List<DialogueLine> conversation2 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Iy bu ne be..."),
                new DialogueLine("Prenses", "Şu çember midir nedir önce onu bulmalıyım."),
            };
            DialogueManager.Instance.StartDialogue(conversation2);
            return;
        }
        if (holdedBodyPart.Value != null)
        {
            List<DialogueLine> conversation3 = new List<DialogueLine>
            {
                new DialogueLine("Prenses", "Ay, şunu da kucakliyim hepsini birden götirim de bitsin hemen..."),
                new DialogueLine("Prenses", "Diyecem de eşşek ölüsü gibi haşmetlim."),
                new DialogueLine("Prenses", "Her neyse önce bunu götüreyim...")
            };
            DialogueManager.Instance.StartDialogue(conversation3);
            return;
        }
        if (bodyCount == 8)
            blockCol.enabled = true;
        List<DialogueLine> conversation = new List<DialogueLine>
            {
                new DialogueLine("Prenses", bodyCount + 1 + ". parçayı buldum.")
            };
        DialogueManager.Instance.StartDialogue(conversation, () =>
        {
            GameObject holding = SpawnAsChildAtTarget(bodyParts[8], holdObjectTransform);
            holdedBodyPart = new KeyValuePair<int, GameObject>(8, holding);
            boddiesToCollect[8].SetActive(false);
        });
    }
    public GameObject SpawnAsChildAtTarget(GameObject prefab, Transform targetTransform)
    {
        if (prefab == null || targetTransform == null)
        {
            Debug.LogWarning("Spawn işlemi başarısız: Prefab veya Hedef Transform eksik!");
            return null;
        }
        GameObject spawnedObject = Instantiate(prefab, targetTransform.position, prefab.transform.rotation, targetTransform);

        return spawnedObject;
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}