import tkinter as tk;
from tkinter import messagebox;
from tkinter import filedialog;
import re;
from PIL import ImageTk, Image;
from os import system as call;
from os.path import isfile;

import saveimageas;

IMAGE_SIZE = (512,512);

DEFAULT_RECENT_SETTINGS = ("0", "0", "1", "128", "128", "max", "jet-inverse");
RECENT_SETTINGS_FILENAME = "_recent_settings.dat";
GENERATION_PROGRAM_PATH = "MandelbrotSet.exe";
IMAGE_FILENAME = "mandelbrot_image.ppm";
DATA_FILENAME = "tmp_data.dat";
ICON_IMAGE_FILENAME = "icon.png";

COLOR_GRADIENT_OPTIONS = (
    "jet-inverse",
    "jet",
    "rgb",
    "greyscale"
);

centerr_entry = centeri_entry = range_entry = definition_entry = maxRD_entry = buffer_size_entry = None;
color_gradient_option_menu = color_gradient_variable = None;
mb_image_label = None;

def set_recent_settings(settings):

    with open(RECENT_SETTINGS_FILENAME, "w") as file:
        file.write(",".join(list(map(lambda x: str(x), settings))));

def get_recent_settings():

    global RECENT_SETTINGS_FILENAME, DEFAULT_RECENT_SETTINGS;

    if isfile(RECENT_SETTINGS_FILENAME):

        contents = None;
        with open(RECENT_SETTINGS_FILENAME, "r") as file:
            contents = file.read();

        settings = tuple(contents.split(","));
        if len(settings) == len(DEFAULT_RECENT_SETTINGS):
            return settings;
        else:
            print(settings);
            return DEFAULT_RECENT_SETTINGS;

    else:
        
        return DEFAULT_RECENT_SETTINGS;

def is_number(string):
    return re.search("^-?[0-9]+\.?[0-9]*$", string) != None;

def check_arguments_validity(centerr,centeri,range,definition,maxRecursionDepth,buffer_size):
    return is_number(centerr) and is_number(centeri) and is_number(range) and definition.isdigit() and maxRecursionDepth.isdigit() and (buffer_size.isdigit() or buffer_size == "max");

def load_icon_image():

    global ICON_IMAGE_FILENAME;

    return ImageTk.PhotoImage(Image.open(ICON_IMAGE_FILENAME));

#Image

SAVE_AS_FILE_TYPES = (
    ("PNG","*.png"),
    ("PPM","*.ppm"),
    ("JPEG","*.jpeg")
);

def on_save_image_button_press():

    global centerr_entry, centeri_entry, range_entry;
    global SAVE_AS_FILE_TYPES;

    centerr = centerr_entry.get();
    centeri = centeri_entry.get();
    range = range_entry.get();

    filename = filedialog.asksaveasfilename(filetype = SAVE_AS_FILE_TYPES,
                                            defaultextension=SAVE_AS_FILE_TYPES,
                                            initialfile = f"{centerr},{centeri} range {range}");

    saveimageas.save_image_as(IMAGE_FILENAME, filename);

def open_image():
    return Image.open(IMAGE_FILENAME);

def recalculate_mb(center,range,definition,maxRecursionDepth,buffer_size):

    global GENERATION_PROGRAM_PATH;

    if buffer_size == "max":
        buffer_size = (2 ** 64) - 1;

    call(f"{GENERATION_PROGRAM_PATH} dv {center[0]} {center[1]} {range} {definition} {maxRecursionDepth} {buffer_size}");

def generate_new_ppm(color_gradient_name):

    call(f"{GENERATION_PROGRAM_PATH} genppm {DATA_FILENAME} {IMAGE_FILENAME} {color_gradient_name}");

def get_mb_image(color_gradient_name, try_use_current = False):

    global IMAGE_SIZE,DATA_FILENAME;

    if not isfile(DATA_FILENAME):

        return tk.PhotoImage();

    else:

        if (not try_use_current) or (not isfile(IMAGE_FILENAME)):

            generate_new_ppm(color_gradient_name);
            image = open_image();
            image = image.resize(IMAGE_SIZE);

        else:

            try:

                if not isfile(IMAGE_FILENAME):
                    raise OSError("Image file doesn't exist"); #This is to forcefully run the "except OSError" code

                image = open_image();
                image = image.resize(IMAGE_SIZE);

            except OSError:

                generate_new_ppm(color_gradient_name);
                image = open_image();
                image = image.resize(IMAGE_SIZE);

        photoImage = ImageTk.PhotoImage(image);
        return photoImage;

def refresh_image():

    global color_gradient_variable;

    new_img = get_mb_image(color_gradient_variable.get(), try_use_current = False);
    mb_image_label.config(image=new_img);
    mb_image_label.image = new_img;

def on_recalculate_press():

    global IMAGE_SIZE;
    global centerr_entry, centeri_entry, range_entry, definition_entry, maxRD_entry, buffer_size_entry;
    global color_gradient_variable;
    global mb_image_label;

    centerr = centerr_entry.get();
    centeri = centeri_entry.get();
    range = range_entry.get();
    definition = definition_entry.get();
    maxRD = maxRD_entry.get();
    buffer_size = buffer_size_entry.get();

    if check_arguments_validity(centerr,centeri,range,definition,maxRD,buffer_size):

        set_recent_settings((centerr,centeri,range,definition,maxRD,buffer_size,color_gradient_variable.get()));

        recalculate_mb((centerr,str(-float(centeri))),range,definition,maxRD,buffer_size);

        refresh_image();

    else:
        messagebox.showwarning("Invalid", "One or more arguments are invalid");

#Main

def open_interface():

    global IMAGE_FILENAME, DATA_FILENAME;
    global centerr_entry, centeri_entry, range_entry, definition_entry, maxRD_entry, buffer_size_entry;
    global color_gradient_option_menu, color_gradient_variable;
    global mb_image_label;

    recent_settings = get_recent_settings();
    
    window = tk.Tk();
    window.title("Mandelbrot Set");
    window.iconphoto(False, load_icon_image());

    color_gradient_variable = tk.StringVar(window);
    color_gradient_variable.set(recent_settings[6]);

    #Image

    image_frame = tk.Frame(window);

    tk.Button(image_frame,
              text = "Save Image As",
              command = on_save_image_button_press).pack();
    
    mb_image = get_mb_image(color_gradient_variable, try_use_current = True);

    mb_image_label = tk.Label(image_frame,
                              image=mb_image,
                              width=IMAGE_SIZE[0],
                              height=IMAGE_SIZE[1]);

    mb_image_label.pack();

    image_frame.grid(row=0, column=0, padx=20, pady=20);

    #Settings

    settings_frame = tk.Frame(window);

    tk.Label(settings_frame, text="Color Scheme").grid(row=0, column=0);
    color_gradient_option_menu = tk.OptionMenu(settings_frame, color_gradient_variable, *COLOR_GRADIENT_OPTIONS);
    color_gradient_option_menu.grid(row=0, column=1);

    tk.Button(settings_frame, text="Recalculate only Image", command=refresh_image).grid(row=1,column=1);

    tk.Label(settings_frame, text="Center (r)").grid(row=2,column=0);
    centerr_entry = tk.Entry(settings_frame);
    centerr_entry.insert(0, recent_settings[0]);
    centerr_entry.grid(row=2,column=1);

    tk.Label(settings_frame, text="Center (i)").grid(row=3,column=0);
    centeri_entry = tk.Entry(settings_frame);
    centeri_entry.insert(0, recent_settings[1]);
    centeri_entry.grid(row=3,column=1);

    tk.Label(settings_frame, text="Range").grid(row=4,column=0);
    range_entry = tk.Entry(settings_frame);
    range_entry.insert(0, recent_settings[2]);
    range_entry.grid(row=4,column=1);

    tk.Label(settings_frame, text="Definition").grid(row=5,column=0);
    definition_entry = tk.Entry(settings_frame);
    definition_entry.insert(0, recent_settings[3]);
    definition_entry.grid(row=5,column=1);

    tk.Label(settings_frame, text="Max Recursion Depth").grid(row=6,column=0);
    maxRD_entry = tk.Entry(settings_frame);
    maxRD_entry.insert(0, recent_settings[4]);
    maxRD_entry.grid(row=6,column=1);

    tk.Label(settings_frame, text="Buffer Size").grid(row=7,column=0);
    buffer_size_entry = tk.Entry(settings_frame);
    buffer_size_entry.insert(0, recent_settings[5]);
    buffer_size_entry.grid(row=7,column=1);

    tk.Button(settings_frame, text="Reclaculate", command=on_recalculate_press).grid(row=8,column=1);

    settings_frame.grid(row=0, column=1, padx=20, pady=20);

    window.bind("<Return>",lambda x: on_recalculate_press());
    
    window.mainloop();

if __name__ == "__main__":
    open_interface();
