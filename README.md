# RevitToTekla

A Revit add-in and Tekla add-in help BIM users can convert Revit native object to Tekla native object


## How to use

Revit

+ This project is not create a .addin file, so u have to use Add-in Manager. However, you also can create .addin for yourself

Tekla

+ It's a window application. You can run it directly with .exe file or use debug mode of Visual Studio

![alt text](https://github.com/baoquylan/RevitToTekla/blob/master/image/demo.gif?raw=true)

[Youtube](https://www.youtube.com/watch?v=Gpn5tDFeGm0)

## Which objects can be converted?
Currently, this add-in can convert objects, including:

Element Wall
+ Basic wall
+ Model in place
+ Polyline wall

Element Beam
+ Concrete beam
+ Polyline beam
+ Steel beam (use default Tekla beam, so it's need to adjust the code to create the beam based on the data)

Element Floor
+ Basic floow
+ Modified floor
+ Floor with openning

Element Column
+ Concrete column
+ Steel column (use default Tekla column, so it's need to adjust the code to create the column based on the data)

## Author

Created by LanBao

## About

This project was created for research purposes, so I am not sure I will maintain or upgrade it


## <a name="license"></a>License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.
