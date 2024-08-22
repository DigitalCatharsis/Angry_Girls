using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class CharacterLauncher : MonoBehaviour
    {
        //Direction
        private Vector3 _offsetEndPostion;
        private Vector3 _offsetStartPoint;
        private Vector3 _directionVector;

        //Traectory
        private GameObject[] _trajectoryDots;

        //Positions
        private Transform[] _positionTransforms;

        [Header("Launching Setup")]
        [SerializeField] private GameObject _positionsContainer;
        [Space(10)]
        [SerializeField] private Vector2 _zoomRange = new Vector2(5f, 10f); // ƒË‡Ô‡ÁÓÌ ÁÛÏ‡
        [Space(10)]
        [SerializeField] private float _forceFactorUp;
        [SerializeField] private float _forceFactorForward;

        [Header("Trajectory")]
        [SerializeField] private Transform _offsetPoint;
        [SerializeField] private GameObject _trajectoryDotPrefab;
        [SerializeField] private int _dotsNumber;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _minDistanceForZoom;

        public void InitLauncher()
        {
            //Init spawnPoints
            var transforms = new HashSet<Transform>(_positionsContainer.GetComponentsInChildren<Transform>());
            transforms.Remove(_positionsContainer.transform); //remove position of _positionsContainer itself
            _positionTransforms = transforms.ToArray();

            //Start position for launch
            _offsetStartPoint = _positionTransforms[0].position;

            //Traectory
            _trajectoryDots = new GameObject[_dotsNumber];
            SpawnProjectoryDots(_positionTransforms[0]);
        }

        public List<GameObject> SpawnAndGetCharacters(CharacterType[] selectedCharactersList)
        {
            var charList = new List<GameObject>();
            for (var i = 0; i < selectedCharactersList.Count(); i++)
            {
                //charList.Add(Instantiate(Resources.Load(selectedCharactersList[i].ToString())) as GameObject); //old
                charList.Add(Singleton.Instance.poolManager.GetObject<CharacterType>
                    (selectedCharactersList[i], Singleton.Instance.poolManager.characterPoolDictionary, Vector3.zero, Quaternion.identity));
            }
            return charList;
        }

        public void UpdateCharacterPositions(List<GameObject> charactersToLaunch)
        {
            for (var i = 0; i < Singleton.Instance.launchManager.charactersToLaunchLeft.Count(); i++)
            {
                charactersToLaunch[i].transform.position = _positionTransforms[i].position;
            }
        }

        public void SetLaunchableCharactersBehavior(List<GameObject> charactersToLaunchLeft)
        {
            foreach (var character in charactersToLaunchLeft)
            {
                character.GetComponent<CControl>().unitBehaviorIsStatic = false;
            }
        }

        private void SpawnProjectoryDots(Transform spawnTransform)
        {
            for (int i = 0; i < _dotsNumber; i++)
            {
                _trajectoryDots[i] = Instantiate(_trajectoryDotPrefab, spawnTransform);
            }
        }

        public void AimingTheLaunch(GameObject characterToLaunch)
        {
            CalculateDirection();
            DrawTraectory();
            AdjustCameraZoom();
        }

        private void CalculateDirection()
        {
            //Calculation direction
            var pointerPosition = Singleton.Instance.ÒameraManager.GetPointerWorldPosition(Camera.main);
            _offsetEndPostion = new Vector3(0, pointerPosition.y, pointerPosition.z);
            _directionVector = _offsetEndPostion - _offsetStartPoint;
        }
        private void DrawTraectory()
        {
            EnableTrajectoryDots();
            _offsetPoint.position = _offsetEndPostion;

            for (var i = 0; i < _dotsNumber; ++i)
            {
                _trajectoryDots[i].transform.position = CalculateTraectoryPosition(i * 0.1f);
            }
        }
        private Vector3 CalculateTraectoryPosition(float elapsedTime)
        {
            return new Vector3(0, _offsetStartPoint.y, _offsetStartPoint.z)
                    + new Vector3(0, -_directionVector.y * _forceFactorUp, -_directionVector.z * _forceFactorForward) * elapsedTime
                    + 0.5f * Physics.gravity * elapsedTime * elapsedTime;
        }

        public void LaunchUnit(CharacterControl characterToLaunch)
        {
            DisableTrajectoryDots();
            characterToLaunch.rigidBody.useGravity = true;
            characterToLaunch.rigidBody.velocity = new Vector3(0, -_directionVector.y * _forceFactorUp, -_directionVector.z * _forceFactorForward);
            StartCoroutine(characterToLaunch.subComponentProcessor.launchLogic.ProcessLaunch());
        }

        private void DisableTrajectoryDots()
        {
            for (int i = 0; i < _trajectoryDots.Length; i++)
            {
                _trajectoryDots[i].SetActive(false);
            }
        } 
        private void EnableTrajectoryDots()
        {
            for (int i = 0; i < _trajectoryDots.Length; i++)
            {
                _trajectoryDots[i].SetActive(true);
            }
        }


        //private Vector3 GetPointerWorldPosition(Camera camera)
        //{
        //    Vector3 screenPosition = Input.mousePosition;
        //    screenPosition.z = camera.nearClipPlane + 1;
        //    return camera.ScreenToWorldPoint(screenPosition);
        //}

        private void AdjustCameraZoom()
        {
            // Calculate distance between _offsetEndPostion and _startPoint
            float distance = Vector3.Distance(_offsetEndPostion, _offsetStartPoint);

            // Calculate zoom based on distance
            float zoomFactor = Mathf.Lerp(_zoomRange.x, _zoomRange.y, distance / _minDistanceForZoom);

            // Apply zoom to camera
            Camera.main.orthographicSize = zoomFactor;
        }



        //public void SwapCharacters(int indexA, int indexB)
        //{
        //    var tmp = _charactersList[indexA];
        //    _charactersList[indexA] = _charactersList[indexB];
        //    _charactersList[indexB] = tmp;

        //    _characterToLaunch = _charactersList[0].GetComponent<CharacterControl>();
        //}
    }
}