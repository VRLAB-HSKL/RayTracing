using UnityEngine;

namespace Sphere
{
    public class UpdateSphereMaterial : MonoBehaviour
    {
        private Color _newColor;

        /// <summary>
        /// Unity Start function
        /// ====================
        /// 
        /// This function is called before the first frame update, after
        /// <see>
        ///     <cref>Awake</cref>
        /// </see>
        /// </summary>
        private void Start()
        {
            _newColor = Color.red;
        }

        /// <summary>
        /// Unity Update function
        /// =====================
        ///
        /// Core game loop, is called once per frame
        /// </summary>
        private void Update()
        {
            _newColor = Random.ColorHSV();
        }
    }
}
