def load_data_file(filename):

    data = [];
    
    with open(filename, "rb") as file:

        #Row length is unsigned int64
        row_length = int.from_bytes(file.read(8), "little", signed=False) - 1;

        current_row = [];

        row_index = 0;
        while True:

            #Each value is unsigned int16
            entry = file.read(2);
            
            if entry == b"":
                break;
            
            value = int.from_bytes(entry, "little", signed=False);
            current_row.append(value);

            if row_index == row_length:
                row_index = 0;
                data.append(current_row);
                current_row = [];
            else:
                row_index += 1;

    return data;