from PIL import Image;
from sys import argv;

def save_image_as(src, filename):

    Image.open(src).save(filename);

def main():

    if len(argv) != 3:
        print("arguments: " + str(argv)); #TODO - remove once finished debugging
        raise Exception("Invalid number of arguments provided");

    src = argv[1];
    filename = argv[2];

    save_image_as(src, filename);

if __name__ == "__main__":
    main();
