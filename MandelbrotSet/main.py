import tkinter as tk;
from tkinter import messagebox;
import re;
from PIL import ImageTk, Image;
from os import system as call;
from os.path import isfile;

import imagegeneration;
import loaddata;

DEFAULT_RECENT_SETTINGS = ("0", "0", "1", "128", "128", "max");
RECENT_SETTINGS_FILENAME = "_recent_settings.dat";
GENERATION_PROGRAM_PATH = "MandelbrotSet.exe";
IMAGE_FILENAME = "mandelbrot_image.ppm";
DATA_FILENAME = "tmp_data.dat";

centerr_entry = centeri_entry = range_entry = definition_entry = maxRD_entry = buffer_size_entry = None;
mb_image_label = None;
status_label = None;

def set_recent_settings(settings):

    with open(RECENT_SETTINGS_FILENAME, "w") as file:
        file.write(",".join(list(map(lambda x: str(x), settings))));

def get_recent_settings():

    global RECENT_SETTINGS_FILENAME, DEFAULT_RECENT_SETTINGS;

    if isfile(RECENT_SETTINGS_FILENAME):

        contents = None;
        with open(RECENT_SETTINGS_FILENAME, "r") as file:
            contents = file.read();

        return tuple(contents.split(","));

    else:

        return DEFAULT_RECENT_SETTINGS;

def is_number(string):
    return re.search("^-?[0-9]+\.?[0-9]*$", string) != None;

def check_arguments_validity(centerr,centeri,range,definition,maxRecursionDepth,buffer_size):
    return is_number(centerr) and is_number(centeri) and is_number(range) and definition.isdigit() and maxRecursionDepth.isdigit() and (buffer_size.isdigit() or buffer_size == "max");

def recalculate_mb(center,range,definition,maxRecursionDepth,buffer_size):

    global GENERATION_PROGRAM_PATH;

    if buffer_size == "max":
        buffer_size = (2 ** 64) - 1;

    call(f"{GENERATION_PROGRAM_PATH} dv {center[0]} {center[1]} {range} {definition} {maxRecursionDepth} {buffer_size}");

def get_mb_image():

    data = loaddata.load_data_file(DATA_FILENAME);
    imagegeneration.write_ppm(data, IMAGE_FILENAME);

    image = Image.open(IMAGE_FILENAME);

    photoImage = ImageTk.PhotoImage(image);
    return photoImage;

def on_recalculate_press():

    global centerr_entry, centeri_entry, range_entry, definition_entry, maxRD_entry, buffer_size_entry;
    global mb_image_label;
    global status_label;

    centerr = centerr_entry.get();
    centeri = centeri_entry.get();
    range = range_entry.get();
    definition = definition_entry.get();
    maxRD = maxRD_entry.get();
    buffer_size = buffer_size_entry.get();

    if check_arguments_validity(centerr,centeri,range,definition,maxRD,buffer_size):

        set_recent_settings((centerr,centeri,range,definition,maxRD,buffer_size));

        recalculate_mb((centerr,str(-float(centeri))),range,definition,maxRD,buffer_size);

        new_img = get_mb_image();
        mb_image_label.config(image=new_img);
        mb_image_label.image = new_img;

    else:
        messagebox.showwarning("Invalid", "One or more arguments are invalid");

def open_interface():

    global IMAGE_FILENAME, DATA_FILENAME;
    global centerr_entry, centeri_entry, range_entry, definition_entry, maxRD_entry, buffer_size_entry;
    global mb_image_label;
    global status_label;

    recent_settings = get_recent_settings();

    window = tk.Tk();
    
    mb_image = get_mb_image();

    mb_image_label = tk.Label(window,
                              image=mb_image,
                              width=256,
                              height=256);

    mb_image_label.pack();

    tk.Label(window, text="Center (r)").pack();
    centerr_entry = tk.Entry(window);
    centerr_entry.insert(0, recent_settings[0]);
    centerr_entry.pack();

    tk.Label(window, text="Center (i)").pack();
    centeri_entry = tk.Entry(window);
    centeri_entry.insert(0, recent_settings[1]);
    centeri_entry.pack();

    tk.Label(window, text="Range").pack();
    range_entry = tk.Entry(window);
    range_entry.insert(0, recent_settings[2]);
    range_entry.pack();

    tk.Label(window, text="Definition").pack();
    definition_entry = tk.Entry(window);
    definition_entry.insert(0, recent_settings[3]);
    definition_entry.pack();

    tk.Label(window, text="Max Recursion Depth").pack();
    maxRD_entry = tk.Entry(window);
    maxRD_entry.insert(0, recent_settings[4]);
    maxRD_entry.pack();

    tk.Label(window, text="Buffer Size").pack();
    buffer_size_entry = tk.Entry(window);
    buffer_size_entry.insert(0, recent_settings[5]);
    buffer_size_entry.pack();

    status_label = tk.Label(window, text="");
    status_label.pack();

    window.bind("<Return>",lambda x: on_recalculate_press());

    tk.Button(window, text="Reclaculate", command=on_recalculate_press).pack();
    
    window.mainloop();

if __name__ == "__main__":
    open_interface();
