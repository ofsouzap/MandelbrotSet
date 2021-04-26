from math import floor;

def normalised_value_to_byte(value):
    return floor(value * 255);

#Converts a normalised value into a 3-value bytearray signifying a red, green and blue value respectively
def normalised_to_color(value):

    #TODO - replace with something better

    return bytearray([
        normalised_value_to_byte(value),
        normalised_value_to_byte(value),
        normalised_value_to_byte(value)
        ]);

#Generates a ppm image from a 2-dimensional iterable with numerical values and saves it as the specified filename
def write_ppm(raw_values, filename):

    NEWLINE = ord("\n");

    rows = len(raw_values[0]);
    columns = len(raw_values);
    
    for column in raw_values:
        if (len(column)) != rows:
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

                normalised_value = value / maximum_value;
                color = normalised_to_color(normalised_value);
                file.write(color);
