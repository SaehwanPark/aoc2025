#!/bin/bash

# Define the start and end days
START_DAY=1
END_DAY=12

# Loop through the day numbers
for i in $(seq $START_DAY $END_DAY); do
    # Format the day number with a leading zero (e.g., 1 -> 01, 12 -> 12)
    # The printf command handles this formatting.
    DAY_NUM=$(printf "%02d" $i)

    # Construct the filename
    FILENAME="day${DAY_NUM}.fsx"

    echo "--- Executing $FILENAME ---"

    # Check if the file exists before attempting to run it
    if [ -f "$FILENAME" ]; then
        # Run the F# script using fsharpi
        dotnet fsi "$FILENAME"

        # Check the exit status of the fsharpi command
        if [ $? -eq 0 ]; then
            echo "$FILENAME executed successfully."
        else
            echo "Error: $FILENAME failed to execute."
            # Optionally, you can exit the script immediately upon failure
            # exit 1
        fi
    else
        echo "Warning: $FILENAME not found. Skipping."
    fi

    echo "--------------------------"
done

echo "Script execution complete."
