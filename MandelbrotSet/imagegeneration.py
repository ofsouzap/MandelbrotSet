from math import floor;
from matplotlib import cm;

colormap = cm.get_cmap("jet");

def normalised_value_to_byte(value):
    return floor(value * 255);

def get_value(self, point):

    if type(point) != float:
        raise Exception("point must be float");

    if (point < 0) or (point > 1):
        raise Exception("point must be in range [0,1]");

#Converts a normalised value into a 3-value bytearray signifying a red, green and blue value respectively
def normalised_to_color(value):

    global colormap;

    if value == 0:
        return bytearray([0,0,0]);

    rgba = colormap(1-value);

    return bytearray([
        normalised_value_to_byte(rgba[0]),
        normalised_value_to_byte(rgba[1]),
        normalised_value_to_byte(rgba[2])
        ]);

#Generates a ppm image from a 2-dimensional iterable with numerical values and saves it as the specified filename
def write_ppm(raw_values, filename):

    columns = len(raw_values[0]);
    rows = len(raw_values);
    
    for column in raw_values:
        if (len(column)) != columns:
            raise Exception("raw_values has inconsistent row count");

    maximum_value = 0;

    for column in raw_values:
        for value in column:
            if value > maximum_value:
                maximum_value = value;

    with open(filename, "wb") as file:

        file.write(b"P6\n");
        file.write(bytearray(f"{columns} {rows}\n","UTF-8"));
        file.write(b"255\n");

        for column in raw_values:

            for value in column:

                if maximum_value != 0:
                    normalised_value = value / maximum_value;
                else:
                    normalised_value = 0 if value == 0 else 1;

                color = normalised_to_color(normalised_value);
                file.write(color);
