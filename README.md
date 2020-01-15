# Naive Bayes Classifiers experimental work

```
Artiemjew, P, Idzikowski, P.: Building an Ensemble of Naive Bayes Classifiers using committee of bootstraps
and monte carlo splits for a various percentage of random objects from training set,CS&P'19: 
Concurrency, Specification and Programming 2019, Olsztyn 2019, accepted
```

## Description
<p align="justify">
In the work we have implemented an ensemble of Naive Bayes classifiers using committee of bootstraps and monte carlo splits. We have conducted 50 iterations of learning in each tested model. Fixed percentage of random objects from the original training system was used. New training decision systems that were considered consisted of 10 to 100 percent of random objects from original training decision system. Two main variants were checked, first with objects returning after the drawn (bootstraps) - and without returning (as monte carlo splits). We have presented how Naive Bayes classifier works in mentioned models on selected data from UCI repository.
</p>

## Repository info
<p align="justify">
Repository consists of two datasets from UCI repository (australian, diabetes) as text files; output example based on australian dataset with results visualization (charts); software including program to perform monte carlo splits based test and bootstrap based test. One way to run software is to use executable files from <b>software/executables only</b> directory and another way is to compile & run VS solutions. Major amount of code is written by using Polish language (did not expect that this will be eligible for scientific publication). Both applications are not that flexible as I would wish for, but both did their purpose to achieve interesting data. Big thanks to dr hab. Piotr Artiemjew for support during software development. Thanks to <a href="https://beam.venngage.com/">BEAM (Chart Maker)</a> for nice web application, allowing to create charts which were used in example output.  
</p>

## Software details

### Running
If you want to run the software by your own, please, keep in mind
1) to place the desired dataset in the directory where .exe file of the program exists,
2) that you are obliged to run BootstrapNB before MonteCarlo as BootstrapNB produces file containing randomized TST data (testowy.txt) which is required in MonteCarlo program (by default MonteCarlo looks after file called: testowyPRE.txt), 
3) if the dataset is different than australian, you need to modify in
   - MonteCarlo program lines: 75, 77, 114, 115, 122 and 123 (last two are not that important to overwrite)
   - BootstrapsNB program lines: 66, 93, 94, 100 and 101 (the same rule with the last two lines like in MonteCarlo program)

### Input
<p align="justify">
Either BootstrapNB or MonteCarlo require to specify percentage of objects in testing set. That value is much more important in BootstrapNB as it produces TST set which is later used by MonteCarlo program. Nevertheless, both console applications should receive the same input data. 
</p>

### Output

Note that,

NN is percentage amount of objects that defines, how many objects were randomized into TRN (training) dataset,

X is an integer value from <1, 50> which responds for iteration number,

committee is value showing global accuracy by following method: collect intel how object was classified during 50 iterations, the most appearing decision is a verdict (if even, object is not recognized as 'classified'), check picture below (keep in mind that this file can be produced as output if uncommented line with ```komitetTabela.txt``` text in console application) 

<p align="center">
<img src="https://github.com/trolit/naive-bayes-classifiers-work/blob/master/img/committee.PNG" alt="Committee image example" width="780px"></img>
</p>

Both console applications produce 
- files named averageNN%, where you can find calculated average parameters like global accuracy, global coverage after one classification process,
- files named global_accNN%, where you can find global accuracy value state after each iteration 
- files named komitetNN%, where you will find committee value state,
- files named treningowyNN%_iX, where you fill objects picked randomly into TRN dataset from specific(X) iteration

File named testowy is created only by BootstrapNB console app to be used in MonteCarlo program.

## Authors
- profesor UWM dr hab. Piotr Artiemjew, <a href="http://wmii.uwm.edu.pl/~artem/teaching.html">homepage</a>
- Paweł Idzikowski, <a href="https://trolit.github.io/">homepage</a>
