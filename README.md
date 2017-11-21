# SilentHttpServer

I have recently been getting more interested in networking. I've for a while been meaning to make a simple HTTP server in C#, and have only just got around to it. So this project is very simple, it has a class called listener, which listens for tcp data coming from port of your choice (preferred 80), it reads it, then sends it to a http request parser. After parsing the http request it then calls a response parser that generates a response for the client. You can do as you wish with the response parser. The response parser is extremely simple and very easy to understand.

So to make a custom response, it is straight forward. Under the folder "Handlers" you will find a class called "WebResponseHandler". It is pre-configured with some examples. To make your own page, you will find a switch inside the WebResponseHandler class, all you need to do is add your own case. You will want to make a template, which can be made in the next paragraph. Back on topic... From within the case, you wamt to set the following code:
```
                    case "/login":
                        {
                            m_template = Template.MergeTemplates(
                                m_template,
                                Templates.Custom.<YourTemplate>.GetTemplate(
                                    ref request,
                                    ref requestData));

                            break;
                        }
```
This code will basically generate a new template, and merge it to the existing template variable. 

## Making a custom template
Now we need to make a template, but how do we do that? First of all we need to make a new class for a template which I recommend you to create under Templates>Custom. After creating your template Adde a public static function called GetTemplate, which you can copy from another template file. From within the template file, you can edit a few things. Firt of all I will go over the basic functions.

### Template Functions
This function will set a HTTP response header
<br>
```SetHeader(string headerName, string headerValue);```
<br><br>
This will set the main header (HTTP/1.1 StatusCode Descriptor) Example: HTTP/1.1 500 Internal Server Error
<br>
```SetStatus(int statusCode, int statusCodeDescriptor);```
<br><br>
Appends content to the output buffer
<br>
```AppendContent(string s);```

# Image
![Unable to load image](https://i.gyazo.com/31e057c1c6f4baf506d570f2afedae41.png)
