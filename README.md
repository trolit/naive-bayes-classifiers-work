# Naive Bayes Classifiers experimental work

```
Artiemjew, P, Idzikowski, P.: Building an Ensemble of Naive Bayes Classifiers using committee of bootstraps
and monte carlo splits for a various percentage of random objects from training set,CS&P'19: 
Concurrency, Specification and Programming 2019, Olsztyn 2019, accepted
```

## Description
<p align="justify">
In the work we have implemented an ensemble of Naive
Bayes classifiers using committee of bootstraps and monte carlo splits.
We have conducted 50 iterations of learning in each tested model. Fixed
percentage of random objects from the original training system was used.
New training decision systems that were considered consisted of 10 to
100 percent of random objects from original training decision system.
Two main variants were checked, first with objects returning after the
drawn (bootstraps) - and without returning (as monte carlo splits). We
have presented how Naive Bayes classifier works in mentioned models on
selected data from UCI repository.
</p>

## Repository info
<p align="justify">
Repository consists of two datasets from UCI repository (australian, diabetes) as text files; output example based on australian dataset with results visualization; software including program to perform monte carlo splits based test and bootstrap based test. One way to run software is to use executable files from <b>software/executables only</b> directory and another way is to compile & run VS solutions. Major amount of code is written by using Polish language (did not expect that this will be eligible for scientific publication). Both applications are not that flexible as I would wish for, but both did their purpose to achieve interesting data.  
</p>

## Software details

### Running
If you want to run the software by your own, please, keep in mind
1) to place the desired dataset in the directory where .exe file of the program exists,
2) that you are obliged to run BootstrapNB before MonteCarlo as BootstrapNB produces file containing randomized TST data (testowy.txt.) which is required in MonteCarlo program (by default MonteCarlo looks after file called: testowyPRE.txt), 
3) if the dataset is different than australian, you need to modify in
   - MonteCarlo program lines: 75, 77, 114, 115, 122 and 123 (last two are not that important to overwrite)
   - BootstrapsNB program lines: 66, 93, 94, 100 and 101 (the same rule with the last two lines like in MonteCarlo program)

### Input
Either BootstrapNB or MonteCarlo require to specify percentage of objects in testing set. That value is much more important in BootstrapNB as it produces TST set which is later used by MonteCarlo program. Nevertheless, both console applications should receive the same input data. 

#### Output


## Authors
- profesor UWM dr hab. Piotr Artiemjew, <a href="http://wmii.uwm.edu.pl/~artem/teaching.html">homepage</a>
- Paweł Idzikowski

Big thanks to <a href="https://beam.venngage.com/">BEAM (Chart Maker)</a> for nice web application, allowing to create charts that were used in example output.

