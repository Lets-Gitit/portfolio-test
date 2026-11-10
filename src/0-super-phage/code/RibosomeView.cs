using TMPro;
using UnityEngine;

namespace SuperPhage
{
    public class RibosomeView : MonoBehaviour
    {
        [SerializeField] private GameObject Gauge;
        [SerializeField] private GameObject AminoAmount;
        [SerializeField] private GameObject AGTCAmount;

        [SerializeField] private SpriteRenderer GaugeFill;
        [SerializeField] private TMP_Text AminoText;
        [SerializeField] private TMP_Text AGTCText;

        [SerializeField] private Transform gaugeFillPivot;
        private Vector3 fullGaugeScale;

        private void Awake()
        {
            fullGaugeScale = gaugeFillPivot.localScale;
            RefreshGauge(0f);
            EraseView();
        }

        public void RefreshAminoCount(int Amino)
        {
            AminoText.text = $"X {Amino}";
        }

        public void RefreshAGTCCount(int AGTC)
        {
            AGTCText.text = $"X {AGTC}";
        }

        public void RefreshGauge(float progress) 
        {
            float clampedProgress = Mathf.Clamp01(progress);

            gaugeFillPivot.localScale = new Vector3(
                fullGaugeScale.x * clampedProgress,
                fullGaugeScale.y,
                fullGaugeScale.z
            );
        }

        public void PhageFactoryView()
        {
            EraseView();

            Gauge.gameObject.SetActive(true);
            AminoAmount.gameObject.SetActive(true);
            AGTCAmount.gameObject.SetActive(true);
        }

        public void NucleaseFactoryView()
        {
            EraseView();

            Gauge.gameObject.SetActive(true);
            AminoAmount.gameObject.SetActive(true);
        }

        public void EraseView()
        {
            Gauge.gameObject.SetActive(false);
            AminoAmount.gameObject.SetActive(false);
            AGTCAmount.gameObject.SetActive(false);

        }
    }

}

