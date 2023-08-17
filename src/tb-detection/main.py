from cv2 import aruco
import argparse

import camera
import repository
import repoStrategy as rs

def get_dict(dict_name):
    aruco_dict = eval(f"aruco.{dict_name}")
    predefined_dict = aruco.getPredefinedDictionary(aruco_dict)
    return predefined_dict

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Detect ArUco markers and send data to Firebase")
    
    parser.add_argument("mode", type=str, choices=["http", "udp"], help="The mode to run the program in")
    parser.add_argument("--url", type=str, default="https://magpietable-default-rtdb.firebaseio.com/", help="The path to the Firebase realtime database found in the Realtime Database tab of the Firebase project page")
    parser.add_argument("--key", type=str, default="./key/firebase_table-key.json", help="The path to the local instance of the Firebase key json file found in the Service Accounts tab of the Firebase project page")
    parser.add_argument("--camera", type=int, default=0, help="The camera index to use")
    parser.add_argument("--aruco_dict", type=str, default="DICT_6X6_50", help="The name of the ArUco dictionary to use")

    args = parser.parse_args()
   
    predefined_dict = get_dict(args.aruco_dict)
    params = aruco.DetectorParameters()

    repoStrategy = rs.RepoStrategyFactory.get_strategy(args.mode)

    repository = repository.Repository(repoStrategy)
    repository.setup()

    camera = camera.Camera(args.camera, aruco_dict_=predefined_dict, params_=params, repository_=repository)
    camera.video_loop()