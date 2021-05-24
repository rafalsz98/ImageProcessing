# Image Processing

## List of content
- [Introduction](#Introduction)
- [Image processing methods](#Image-processing-methods)
- - [Regionprops](#Regionprops)
- - [Kirsch filtration](#Kirsch-filtration)
- - [Opening with circular structural element](#Opening-with-circular-structural-element)
- - [Labeling](#Labeling)

## Introduction
App is written in C# for university course "Analysis and processing digital images". It contains set of
image processing algorithms that can be applied to any given input image.<br>
![obraz](https://user-images.githubusercontent.com/18229762/119397145-00f4d280-bcd6-11eb-8b0f-6daec0eff1f3.png)

## Image processing methods

Below are introduced and explained image processing algorithms

### Regionprops
For a given input image in monochrome it returns txt file with statiscics about every pixel. Here we have:
- ID - it is value of a pixel from a range 0 - 255
- Centroid - It is sum of x's positions for a given value, divided by quantity of them, similar for y's.
- Bounding box - 2 first coordinates contains left-top corner of the rectangle, which contains every pixel with given value, the next 2
is width and height
- Equivalent diameter - diameter of the circle with the same area as region. Counted by 4 * Area / Pi

#### Example usage:
![obraz](https://user-images.githubusercontent.com/18229762/119397667-c2abe300-bcd6-11eb-8e87-bfd9f2e84390.png)
<br>Orginal image:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119397810-f4bd4500-bcd6-11eb-8830-f0a54f1ba3af.png)
<br>data.txt:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119397765-e4a56580-bcd6-11eb-971d-591d0ae9bcae.png)

### Kirsch filtration
Algorithm uses 8 matrixes 3x3, starting from the matrix:<br>
```
  -3  -3  5
  -3  0   5
  -3  -3  5
```

Every next matrix is a result of rotating above matrix by 45 degrees
Then for every pixel in input image we:
- Sum for every RGB layer value of pixels in neighbourhood of changed pixel and multiplicate it by corresponding value from Kirsch matrix
- Find maximum from all 8 matrices. It is new value of the pixel

#### Example usage:
Input image as above<br>
![obraz](https://user-images.githubusercontent.com/18229762/119399602-626a7080-bcd9-11eb-857e-8c1791f40ebc.png)
<br>
Results:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119399639-6c8c6f00-bcd9-11eb-88cf-b7db805668e1.png)

-----

Input image:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119399754-90e84b80-bcd9-11eb-9da3-b265359eb718.png)

Output image (Layer R):<br>
![obraz](https://user-images.githubusercontent.com/18229762/119400320-5206c580-bcda-11eb-8d0a-300ff641737e.png)
<br>
Output image (Layer G):<br>
![obraz](https://user-images.githubusercontent.com/18229762/119400361-5c28c400-bcda-11eb-886d-9a0ce8565105.png)
<br>
Output image (Layer B):<br>
![obraz](https://user-images.githubusercontent.com/18229762/119400430-75ca0b80-bcda-11eb-830e-9864531f3bcc.png)

### Opening with circular structural element

The opening is the application of erosion to the image followed by dilation.<br>
Erosion is to find the minimum for each pixel within the structural element, while dilation is to find the maximum.

#### Example usage:
Input image:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119400845-01439c80-bcdb-11eb-8940-0aaff4b0352e.png)<br>
![obraz](https://user-images.githubusercontent.com/18229762/119400859-06085080-bcdb-11eb-8107-5061b67c26f9.png)<br>

(With structural element with radius 5) Results:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119400923-1a4c4d80-bcdb-11eb-8668-6f64544befff.png)

----
Input image:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119401006-3223d180-bcdb-11eb-80ef-7ba820771678.png)<br>

(With structural element with radius 5) Results:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119401034-3e0f9380-bcdb-11eb-878d-be48db85f381.png)

### Labeling
Input image:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119401112-597a9e80-bcdb-11eb-89e8-8af739f17b41.png)

Image has size 20x20. As output we get txt file with numeration for components.

Results:<br>
![obraz](https://user-images.githubusercontent.com/18229762/119401189-72834f80-bcdb-11eb-961c-4d2fc3dee2fc.png)

(For neighbourhood = 4):<br>
![obraz](https://user-images.githubusercontent.com/18229762/119401229-829b2f00-bcdb-11eb-9ef8-25ce76beb2b8.png)

(For enighbourhood = 8): <br>
![obraz](https://user-images.githubusercontent.com/18229762/119401252-8af36a00-bcdb-11eb-8814-7ee1154c4765.png)

