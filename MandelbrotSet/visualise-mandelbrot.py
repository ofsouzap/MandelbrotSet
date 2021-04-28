from sys import argv;
import matplotlib.pyplot as plt;

import loaddata;

def main():

    if len(argv) >= 2:
        filename = argv[1];
    else:
        filename = input("Filename> ");
    
    data = loaddata.load_data_file(filename);

    plt.axis(False);
    plt.pcolormesh(data, cmap="gist_stern");
    plt.show();
    
if __name__ == "__main__":
    main();
