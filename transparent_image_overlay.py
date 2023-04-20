import cv2
import os
import numpy as np

image_path = "./ImageExport"
# alpha = 0.5
# beta = 0.5
# gamma = -50
sequence_length = 8
window_name = 'Blending Parameters'


def create_trackbars():
    cv2.namedWindow(window_name)
    cv2.createTrackbar('Alpha', window_name, 60, 120, nothing)
    cv2.createTrackbar('Beta', window_name, 60, 120, nothing)
    cv2.createTrackbar('Gamma', window_name, 60, 120, nothing)


def nothing(x):
    return


def main(save_images=False):
    # create_trackbars()
    file_names = os.listdir(image_path)
    file_names = [f for f in file_names if f.endswith(".png")]
    background_image = cv2.imread(os.path.join("background_image.png"))
    background_image_original_shape = background_image.shape
    background_image = background_image[:, 450:950]

    for idx, n in enumerate(file_names[:-sequence_length]):
        alpha = 0.6 # cv2.getTrackbarPos('Alpha', window_name) / 100
        beta = 1.0  # cv2.getTrackbarPos('Beta', window_name) / 100
        gamma = -5 # (cv2.getTrackbarPos('Gamma', window_name) - 60)
        # print(alpha, beta, gamma)

        image = cv2.imread(os.path.join(image_path, n))
        # convert screenshot to background image shape
        image = cv2.resize(image, (background_image_original_shape[1], background_image_original_shape[0]))
        image = image[:, 450:950]
        for seq_idx in range(1, sequence_length+1):
            image_add = cv2.imread(os.path.join(image_path, file_names[idx+seq_idx]))
            image_add = cv2.resize(image_add, (background_image_original_shape[1], background_image_original_shape[0]))
            image_add = image_add[:, 450:950]
            image = cv2.addWeighted(image, alpha, image_add, beta, gamma)
            
        image = cv2.addWeighted(background_image, 0.3, image, 1, 0)
        if save_images:
            if not os.path.isdir(os.path.join(image_path, "output")):
                os.makedirs(os.path.join(image_path, "output"))
            cv2.imwrite(os.path.join(image_path, "output", n), image)
        else:
            cv2.imshow("Image", image)
            if cv2.waitKey(500) & 0xFF == ord('q'):
                break
    cv2.destroyAllWindows()


if __name__ == '__main__':
    main(False)