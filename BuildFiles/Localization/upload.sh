#!/bin/bash

# Go down to Dev root

cd ../../..
if [ -d temp ]; then
  rm -rf temp/*
else
  mkdir temp
fi

# Create structure

mkdir temp/Site
mkdir temp/Logic

# Copy Site resources

echo "Copying resources to temporary folder..."

cp Swarmops/Site5/App_GlobalResources/*.resx temp/Site
cp Swarmops/Logic/App_GlobalResources/*.resx temp/Logic
cp Swarmops/BuildFiles/Localization/crowdin.yaml temp

echo "Uploading en-US source files..."

cd temp
crowdin-cli upload sources
cd ..
rm -rf temp

cd Swarmops/BuildFiles/Localization
