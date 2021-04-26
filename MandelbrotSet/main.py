import tkinter as tk;
from tkinter import messagebox;
import re;
from os import system as call;

import imagegeneration;
import loaddata;

GENERATION_PROGRAM_PATH = "MandelbrotSet.exe";
IMAGE_FILENAME = "mandelbrot_image.ppm";
DATA_FILENAME = "tmp_data.dat";

centerr_entry = centeri_entry = range_entry = definition_entry = maxRD_entry = buffer_size_entry = None;
mb_image_label = None;

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

    return tk.PhotoImage(file=IMAGE_FILENAME);

def on_recalculate_press():

    global centerr_entry, centeri_entry, range_entry, definition_entry, maxRD_entry, buffer_size_entry;
    global mb_image_label;

    centerr = centerr_entry.get();
    centeri = centeri_entry.get();
    range = range_entry.get();
    definition = definition_entry.get();
    maxRD = maxRD_entry.get();
    buffer_size = buffer_size_entry.get();

    if check_arguments_validity(centerr,centeri,range,definition,maxRD,buffer_size):

        recalculate_mb((centerr,centeri),range,definition,maxRD,buffer_size);

        new_img = get_mb_image();
        mb_image_label.config(image=new_img);
        mb_image_label.image = new_img;

    else:
        messagebox.showwarning("Invalid", "One or more arguments are invalid");

def open_interface():

    global IMAGE_FILENAME, DATA_FILENAME;
    global centerr_entry, centeri_entry, range_entry, definition_entry, maxRD_entry, buffer_size_entry;
    global mb_image_label;

    window = tk.Tk();
    
    mb_image = get_mb_image();

    mb_image_label = tk.Label(window,
                           image=mb_image);

    mb_image_label.pack(fill=None,expand=False);

    tk.Label(window, text="Center (r)").pack();
    centerr_entry = tk.Entry(window);
    centerr_entry.insert(0, "0");
    centerr_entry.pack();

    tk.Label(window, text="Center (i)").pack();
    centeri_entry = tk.Entry(window);
    centeri_entry.insert(0, "0");
    centeri_entry.pack();

    tk.Label(window, text="Range").pack();
    range_entry = tk.Entry(window);
    range_entry.insert(0, "1");
    range_entry.pack();

    tk.Label(window, text="Definition").pack();
    definition_entry = tk.Entry(window);
    definition_entry.insert(0, "128");
    definition_entry.pack();

    tk.Label(window, text="Max Recursion Depth").pack();
    maxRD_entry = tk.Entry(window);
    maxRD_entry.insert(0, "128");
    maxRD_entry.pack();

    tk.Label(window, text="Buffer Size").pack();
    buffer_size_entry = tk.Entry(window);
    buffer_size_entry.insert(0, "max");
    buffer_size_entry.pack();

    window.bind("<Return>",lambda x: on_recalculate_press());

    tk.Button(window, text="Reclaculate", command=on_recalculate_press).pack();
    
    window.mainloop();

if __name__ == "__main__":
    open_interface();
